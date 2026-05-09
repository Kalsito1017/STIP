"""
STIP ML Training Pipeline
Trains XGBoostRegressor on delay_logs from PostgreSQL.
Produces versioned model artifacts and metadata.
"""

import os
import sys
import json
import logging
from datetime import datetime, timedelta, timezone

import joblib
import numpy as np
import pandas as pd

from app.db import get_connection_params

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
)
logger = logging.getLogger(__name__)

MODEL_DIR = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "models")
FEATURE_NAMES = [
    "hour_of_day",
    "day_of_week",
    "is_peak_hour",
    "is_weekend",
    "route_id_encoded",
    "stop_sequence",
    "historical_avg_delay",
    "weather_score",
]
PEAK_HOURS = {7, 8, 9, 17, 18, 19}


# ---------------------------------------------------------------------------
# DB helpers
# ---------------------------------------------------------------------------

def _get_connection():
    """Create a psycopg2 connection from DB_CONNECTION_STRING env var."""
    import psycopg2

    params = get_connection_params()
    if not params:
        raise RuntimeError("DB_CONNECTION_STRING environment variable is not set")

    logger.debug("Connecting to PostgreSQL")
    return psycopg2.connect(**params)


# ---------------------------------------------------------------------------
# Data loading
# ---------------------------------------------------------------------------

def load_delay_logs(days: int = 90) -> pd.DataFrame:
    """Fetch delay_logs from the last N days, joining stop_times for stop_sequence."""
    conn = _get_connection()
    try:
        query = """
            SELECT
                dl.route_id,
                dl.stop_id,
                dl.trip_id,
                dl.scheduled_arrival,
                dl.actual_arrival,
                dl.delay_seconds,
                COALESCE(st.stop_sequence, 0) AS stop_sequence
            FROM delay_logs dl
            LEFT JOIN stop_times st
                ON dl.trip_id = st.trip_id AND dl.stop_id = st.stop_id
            WHERE dl.recorded_at >= %s
              AND dl.delay_seconds IS NOT NULL
            ORDER BY dl.scheduled_arrival
        """
        cutoff = datetime.now(timezone.utc) - timedelta(days=days)
        logger.debug("Fetching delay_logs from last %d days (since %s)", days, cutoff.isoformat())
        df = pd.read_sql_query(query, conn, params=(cutoff,))
        logger.debug("Fetched %d rows from delay_logs", len(df))
        return df
    finally:
        conn.close()


# ---------------------------------------------------------------------------
# Feature engineering
# ---------------------------------------------------------------------------

def build_route_encoding(df: pd.DataFrame) -> dict[str, int]:
    """Build a deterministic mapping from route_id to integer encoding."""
    unique_routes = sorted(df["route_id"].dropna().unique())
    encoding = {route: idx for idx, route in enumerate(unique_routes)}
    logger.debug("Built route encoding map for %d routes", len(encoding))
    return encoding


def compute_historical_avg_delay(df: pd.DataFrame) -> pd.DataFrame:
    """Compute the mean delay grouped by (route_id, hour_of_day).

    Returns a DataFrame with columns ['route_id', 'hour_of_day', 'historical_avg_delay'].
    """
    stats = (
        df.groupby(["route_id", "hour_of_day"])["delay_seconds"]
        .mean()
        .reset_index()
        .rename(columns={"delay_seconds": "historical_avg_delay"})
    )
    logger.debug("Computed historical_avg_delay for %d (route, hour) groups", len(stats))
    return stats


def engineer_features(
    df: pd.DataFrame,
    route_encoding: dict[str, int],
    historical_stats: pd.DataFrame | None = None,
) -> pd.DataFrame:
    """Add all model features to the DataFrame (in-place)."""
    df["hour_of_day"] = pd.to_datetime(df["scheduled_arrival"]).dt.hour
    df["day_of_week"] = pd.to_datetime(df["scheduled_arrival"]).dt.dayofweek
    df["is_peak_hour"] = df["hour_of_day"].isin(PEAK_HOURS).astype(int)
    df["is_weekend"] = (df["day_of_week"] >= 5).astype(int)
    df["route_id_encoded"] = df["route_id"].map(route_encoding).fillna(0).astype(int)

    # stop_sequence — already populated from join, default to 0 if missing
    if "stop_sequence" not in df.columns:
        df["stop_sequence"] = 0
    df["stop_sequence"] = df["stop_sequence"].fillna(0).astype(int)

    # historical_avg_delay — merge from stats if available
    if historical_stats is not None and len(historical_stats) > 0:
        df = df.merge(historical_stats, on=["route_id", "hour_of_day"], how="left")
        df["historical_avg_delay"] = df["historical_avg_delay"].fillna(0)
    else:
        df["historical_avg_delay"] = 0

    # weather_score — stubbed to 0 (future integration)
    df["weather_score"] = 0

    return df


# ---------------------------------------------------------------------------
# Training
# ---------------------------------------------------------------------------

def train(delay_logs: list[dict] | None = None) -> str:
    """Main training entry point.

    If *delay_logs* is provided (from /internal/retrain), use that data directly.
    Otherwise, fetch from PostgreSQL.

    Returns the model version string, or empty string on failure.
    """
    # ── Load data ────────────────────────────────────────────────────────
    if delay_logs is not None:
        if len(delay_logs) < 10:
            logger.warning("Insufficient data: %d records (need >= 10)", len(delay_logs))
            return ""
        df = pd.DataFrame(delay_logs)
        # stop_sequence may come from the payload or default to 0
        if "stop_sequence" not in df.columns:
            df["stop_sequence"] = 0
    else:
        df = load_delay_logs(days=90)
        if len(df) < 10:
            logger.warning("Insufficient data from DB: %d records", len(df))
            return ""

    logger.info("Training with %d records", len(df))

    # ── Build encoding ───────────────────────────────────────────────────
    route_encoding = build_route_encoding(df)

    # ── Feature engineering (before split for encoding, but stats on train only) ─
    df = engineer_features(df, route_encoding, historical_stats=None)

    X = df[FEATURE_NAMES + ["route_id"]].copy()
    y = df["delay_seconds"].copy()

    # ── Time-based 80/20 split ───────────────────────────────────────────
    split_idx = int(len(X) * 0.8)
    X_train, X_val = X.iloc[:split_idx], X.iloc[split_idx:]
    y_train, y_val = y.iloc[:split_idx], y.iloc[split_idx:]
    logger.debug("Split: %d train / %d validation (time-based)", len(X_train), len(X_val))

    # ── Compute historical stats on training data only (prevent leakage) ─
    train_hist_df = df.iloc[:split_idx][["route_id", "hour_of_day", "delay_seconds"]].copy()
    historical_stats = compute_historical_avg_delay(train_hist_df)

    # Merge historical stats into train and val (using train-only stats for both)
    # Drop pre-existing historical_avg_delay (set to 0 by engineer_features) so the merge
    # adds it cleanly without _x/_y suffix collisions
    X_train = X_train.drop(columns=["historical_avg_delay"]).merge(
        historical_stats, on=["route_id", "hour_of_day"], how="left"
    )
    X_train["historical_avg_delay"] = X_train["historical_avg_delay"].fillna(0)
    X_val = X_val.drop(columns=["historical_avg_delay"]).merge(
        historical_stats, on=["route_id", "hour_of_day"], how="left"
    )
    X_val["historical_avg_delay"] = X_val["historical_avg_delay"].fillna(0)

    # Drop route_id before training (not a feature, only used for merging stats)
    X_train = X_train.drop(columns=["route_id"])
    X_val = X_val.drop(columns=["route_id"])

    # ── Train XGBoost ────────────────────────────────────────────────────
    from xgboost import XGBRegressor

    model = XGBRegressor(
        n_estimators=100,
        max_depth=5,
        learning_rate=0.1,
        objective="reg:squarederror",
        random_state=42,
        n_jobs=-1,
    )
    model.fit(
        X_train,
        y_train,
        eval_set=[(X_val, y_val)],
        verbose=False,
    )

    # ── Evaluate ─────────────────────────────────────────────────────────
    val_preds = model.predict(X_val)
    residuals = y_val.values - val_preds

    mae = float(np.mean(np.abs(residuals)))
    rmse = float(np.sqrt(np.mean(residuals ** 2)))
    ss_res = float(np.sum(residuals ** 2))
    ss_tot = float(np.sum((y_val.values - np.mean(y_val.values)) ** 2))
    r_squared = float(1 - ss_res / ss_tot) if ss_tot > 0 else 0.0

    residual_mean = float(np.mean(residuals))
    residual_std = float(np.std(residuals))

    logger.info(
        "Evaluation — MAE: %.2f s, RMSE: %.2f s, R²: %.4f",
        mae, rmse, r_squared,
    )
    logger.info("Residual stats — mean: %.2f, std: %.2f", residual_mean, residual_std)

    # ── Save artifacts ───────────────────────────────────────────────────
    os.makedirs(MODEL_DIR, exist_ok=True)
    version = f"v{datetime.now(timezone.utc).strftime('%Y%m%d%H%M%S')}"

    model_filename = f"xgb_delay_{version}.joblib"
    model_path = os.path.join(MODEL_DIR, model_filename)
    joblib.dump(model, model_path)
    logger.info("Model saved: %s", model_path)

    # Update symlink to latest
    latest_link = os.path.join(MODEL_DIR, "xgb_delay_latest.joblib")
    if os.path.lexists(latest_link):
        os.remove(latest_link)
    # Use relative path so symlink works across container mounts
    os.symlink(model_filename, latest_link)

    # Save metadata
    meta = {
        "version": version,
        "trained_at": datetime.now(timezone.utc).isoformat(),
        "feature_names": FEATURE_NAMES,
        "feature_count": len(FEATURE_NAMES),
        "sample_count": len(df),
        "train_count": len(X_train),
        "val_count": len(X_val),
        "mae": mae,
        "rmse": rmse,
        "r_squared": r_squared,
        "residual_mean": residual_mean,
        "residual_std": residual_std,
        "route_encoding": route_encoding,
        "model_file": model_filename,
    }
    meta_path = os.path.join(MODEL_DIR, "model_meta.json")
    with open(meta_path, "w") as f:
        json.dump(meta, f, indent=2)
    logger.debug("Metadata saved: %s", meta_path)

    # ── Cleanup old models (keep latest 5) ───────────────────────────────
    _cleanup_old_models(keep=5)

    return version


def _cleanup_old_models(keep: int = 5) -> None:
    """Remove old .joblib model files, keeping the *keep* most recent."""
    import glob as globmod

    pattern = os.path.join(MODEL_DIR, "xgb_delay_v*.joblib")
    files = sorted(globmod.glob(pattern), key=os.path.getmtime, reverse=True)
    for old_file in files[keep:]:
        logger.debug("Removing old model: %s", old_file)
        os.remove(old_file)


# ---------------------------------------------------------------------------
# CLI entry point
# ---------------------------------------------------------------------------

if __name__ == "__main__":
    db_conn = os.environ.get("DB_CONNECTION_STRING", "")
    if not db_conn:
        logger.error("DB_CONNECTION_STRING not set — cannot train from database")
        sys.exit(1)

    version = train()
    if version:
        logger.info("Training complete — model version: %s", version)
    else:
        logger.error("Training failed or insufficient data")
        sys.exit(1)
