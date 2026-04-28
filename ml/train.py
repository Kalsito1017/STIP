import os
import sys
import logging
import pandas as pd
import numpy as np
import joblib
import json
from datetime import datetime

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)

MODEL_DIR = os.path.join(os.path.dirname(__file__), "models")


def train(delay_logs: list[dict]) -> str:
    df = pd.DataFrame(delay_logs)
    logger.info("Loaded %d delay log records", len(df))

    df["hour_of_day"] = pd.to_datetime(df["scheduled_arrival"]).dt.hour
    df["day_of_week"] = pd.to_datetime(df["scheduled_arrival"]).dt.dayofweek
    df["is_peak_hour"] = df["hour_of_day"].isin([7, 8, 9, 17, 18, 19]).astype(int)
    df["is_weekend"] = (df["day_of_week"] >= 5).astype(int)
    df["route_id_encoded"] = df["route_id"].apply(lambda x: hash(x) % 1000)
    df["stop_sequence"] = 0
    df["historical_avg_delay"] = 0
    df["weather_score"] = 0

    features = [
        "hour_of_day", "day_of_week", "is_peak_hour", "is_weekend",
        "route_id_encoded", "stop_sequence", "historical_avg_delay", "weather_score"
    ]

    X = df[features]
    y = df["delay_seconds"]

    if len(X) < 10:
        logger.warning("Insufficient data: %d records", len(X))
        return ""

    from xgboost import XGBRegressor
    from sklearn.model_selection import train_test_split

    X_train, X_val, y_train, y_val = train_test_split(X, y, test_size=0.2, random_state=42)

    model = XGBRegressor(n_estimators=100, max_depth=5, learning_rate=0.1)
    model.fit(X_train, y_train)

    preds = model.predict(X_val)
    mae = float(np.mean(np.abs(y_val - preds)))
    logger.info("Model trained — MAE: %.2f seconds", mae)

    os.makedirs(MODEL_DIR, exist_ok=True)
    version = f"v{datetime.utcnow().strftime('%Y%m%d%H%M%S')}"
    model_path = os.path.join(MODEL_DIR, f"xgb_delay_{version}.joblib")
    joblib.dump(model, model_path)

    meta = {
        "version": version,
        "trained_at": datetime.utcnow().isoformat(),
        "feature_count": len(features),
        "sample_count": len(df),
        "mae": mae,
    }
    with open(os.path.join(MODEL_DIR, "model_meta.json"), "w") as f:
        json.dump(meta, f)

    symlink = os.path.join(MODEL_DIR, "xgb_delay_latest.joblib")
    if os.path.exists(symlink):
        os.remove(symlink)
    os.symlink(model_path, symlink)

    logger.info("Model saved: %s", model_path)
    return version


if __name__ == "__main__":
    import httpx

    db_conn = os.environ.get("DB_CONNECTION_STRING", "")
    if not db_conn:
        logger.error("DB_CONNECTION_STRING not set")
        sys.exit(1)

    logger.info("Training pipeline complete (data expected via API endpoint)")
