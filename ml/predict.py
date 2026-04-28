"""
STIP ML Prediction Service
FastAPI inference server for bus delay prediction.
"""

import glob as globmod
import json
import logging
import os
from contextlib import asynccontextmanager
from datetime import datetime, timezone

import joblib
import numpy as np
import pandas as pd
from fastapi import FastAPI, HTTPException, Header, Request
from pydantic import BaseModel, Field
from slowapi import Limiter
from slowapi.util import get_remote_address

from db import get_connection_params

ML_INTERNAL_SECRET = os.environ.get("ML_INTERNAL_SECRET", "")

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
)
logger = logging.getLogger(__name__)

MODEL_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "models")
META_PATH = os.path.join(MODEL_DIR, "model_meta.json")

PEAK_HOURS = {7, 8, 9, 17, 18, 19}
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

# Module-level state loaded at startup
_model = None
_model_meta: dict = {}
_route_encoding: dict[str, int] = {}
_residual_std: float = 30.0  # fallback
_residual_mean: float = 0.0


# ---------------------------------------------------------------------------
# Model loading
# ---------------------------------------------------------------------------

def _safe_model_path(path: str) -> str:
    """Validate that a model file path is within MODEL_DIR (prevent path traversal)."""
    resolved = os.path.realpath(path)
    model_dir_resolved = os.path.realpath(MODEL_DIR)
    if not resolved.startswith(model_dir_resolved + os.sep) and resolved != model_dir_resolved:
        raise ValueError(f"Model path outside MODEL_DIR: {path}")
    return resolved


def _find_latest_model_path() -> str | None:
    """Find the latest versioned model file in MODEL_DIR.

    Strategy:
    1. Read model_meta.json → use 'model_file' key if present
    2. Otherwise, fall back to symlink xgb_delay_latest.joblib
    3. Otherwise, pick newest xgb_delay_v*.joblib by mtime
    """
    # 1) model_meta.json
    if os.path.exists(META_PATH):
        try:
            with open(META_PATH) as f:
                meta = json.load(f)
            model_file = meta.get("model_file")
            if model_file:
                path = os.path.join(MODEL_DIR, model_file)
                try:
                    path = _safe_model_path(path)
                except ValueError:
                    logger.warning("model_file path traversal blocked: %s", model_file)
                    return None
                if os.path.exists(path):
                    return path
        except (json.JSONDecodeError, OSError):
            pass

    # 2) symlink
    symlink = os.path.join(MODEL_DIR, "xgb_delay_latest.joblib")
    if os.path.exists(symlink):
        resolved = os.path.realpath(symlink)
        if os.path.exists(resolved):
            return resolved

    # 3) newest by mtime
    pattern = os.path.join(MODEL_DIR, "xgb_delay_v*.joblib")
    candidates = sorted(globmod.glob(pattern), key=os.path.getmtime, reverse=True)
    if candidates:
        return candidates[0]

    # Legacy fallback: the old hardcoded name
    legacy = os.path.join(MODEL_DIR, "xgb_delay_v1.joblib")
    if os.path.exists(legacy):
        return legacy

    return None


def load_model() -> None:
    """Load the latest model and metadata into module-level globals."""
    global _model, _model_meta, _route_encoding, _residual_std, _residual_mean

    model_path = _find_latest_model_path()
    if model_path and os.path.exists(model_path):
        try:
            safe_path = _safe_model_path(model_path)
            _model = joblib.load(safe_path)
            logger.info("Loaded model: %s", safe_path)
        except ValueError as e:
            logger.error("Model path validation failed: %s", e)
            _model = None
    else:
        _model = None
        logger.warning("No model found in %s", MODEL_DIR)

    if os.path.exists(META_PATH):
        try:
            with open(META_PATH) as f:
                _model_meta = json.load(f)
        except (json.JSONDecodeError, OSError):
            _model_meta = {}
    else:
        _model_meta = {}

    _route_encoding = _model_meta.get("route_encoding", {})
    _residual_std = _model_meta.get("residual_std", 30.0)
    _residual_mean = _model_meta.get("residual_mean", 0.0)


# ---------------------------------------------------------------------------
# DB helper for historical_avg_delay lookup
# ---------------------------------------------------------------------------

_db_pool = None


def _get_db_pool():
    """Get or create a threaded connection pool."""
    global _db_pool
    if _db_pool is not None:
        return _db_pool
    params = get_connection_params()
    if not params:
        return None
    try:
        from psycopg2 import pool as pg_pool
        max_conn = int(os.environ.get("DB_POOL_MAX", "20"))
        _db_pool = pg_pool.ThreadedConnectionPool(5, max_conn, **params)
        return _db_pool
    except Exception as e:
        logger.debug("Could not create DB pool: %s", e)
        return None


def _lookup_historical_avg_delay(route_id: str, hour: int) -> float:
    """Query PostgreSQL for the 7-day rolling average delay for a route+hour.

    Returns 0.0 on any failure (DB unavailable, no data, etc.).
    """
    pool = _get_db_pool()
    if pool is None:
        return 0.0

    conn = None
    try:
        conn = pool.getconn()
        with conn.cursor() as cur:
            cur.execute(
                """
                SELECT COALESCE(AVG(delay_seconds), 0)
                FROM delay_logs
                WHERE route_id = %s
                  AND EXTRACT(HOUR FROM scheduled_arrival) = %s
                  AND recorded_at >= NOW() - INTERVAL '7 days'
                  AND delay_seconds IS NOT NULL
                """,
                (route_id, hour),
            )
            row = cur.fetchone()
            return float(row[0]) if row and row[0] else 0.0
    except Exception as e:
        logger.debug("Could not query historical_avg_delay: %s", e)
        return 0.0
    finally:
        if conn is not None:
            pool.putconn(conn)


# ---------------------------------------------------------------------------
# FastAPI app
# ---------------------------------------------------------------------------

@asynccontextmanager
async def lifespan(app: FastAPI):
    load_model()
    yield


app = FastAPI(title="STIP ML Service", version="2.0.0", lifespan=lifespan)
limiter = Limiter(key_func=get_remote_address)
app.state.limiter = limiter


# ---------------------------------------------------------------------------
# Request / Response models
# ---------------------------------------------------------------------------

class PredictionRequest(BaseModel):
    route_id: str = Field(..., min_length=1, max_length=50)
    stop_id: str = Field(..., min_length=1, max_length=50)
    hour: int = Field(..., ge=0, le=23)
    day_of_week: int = Field(..., ge=0, le=6)  # 0=Mon .. 6=Sun
    stop_sequence: int = Field(..., ge=0, le=1000)


class PredictionResponse(BaseModel):
    predicted_delay_seconds: float
    confidence_interval: list[float]
    model_version: str


class RetrainRequest(BaseModel):
    delay_logs: list[dict] = Field(..., min_length=1, max_length=50_000)


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@app.get("/health")
def health():
    return {
        "status": "ok",
        "model_loaded": _model is not None,
        "model_version": _model_meta.get("version", "none"),
    }


@app.post("/predict", response_model=PredictionResponse)
@limiter.limit("100/minute")
def predict(request: Request, body: PredictionRequest):
    if _model is None:
        raise HTTPException(status_code=503, detail="Model not loaded")

    # Encode route_id using the training-time mapping
    route_encoded = _route_encoding.get(body.route_id, 0)

    # Look up historical average delay from DB (best-effort)
    hist_avg = _lookup_historical_avg_delay(body.route_id, body.hour)

    features = pd.DataFrame([[
        body.hour,
        body.day_of_week,
        1 if body.hour in PEAK_HOURS else 0,
        1 if body.day_of_week >= 5 else 0,
        route_encoded,
        body.stop_sequence,
        hist_avg,
        0,  # weather_score — future
    ]], columns=FEATURE_NAMES)

    pred = float(_model.predict(features)[0])
    pred = max(0.0, pred)

    # Confidence interval: ±1.96 * residual_std (≈95% CI)
    ci_half = 1.96 * abs(_residual_std)
    lower = max(0.0, pred - ci_half)
    upper = pred + ci_half

    return PredictionResponse(
        predicted_delay_seconds=pred,
        confidence_interval=[round(lower, 2), round(upper, 2)],
        model_version=_model_meta.get("version", "unknown"),
    )


@app.post("/internal/retrain")
def retrain(request: RetrainRequest, x_internal_secret: str = Header(default="")):
    if ML_INTERNAL_SECRET and x_internal_secret != ML_INTERNAL_SECRET:
        raise HTTPException(status_code=403, detail="Forbidden")

    if not request.delay_logs:
        return {"status": "skipped", "reason": "no data"}

    from train import train as run_training

    try:
        version = run_training(delay_logs=request.delay_logs)
    except Exception as e:
        logger.error("Retrain failed: %s", e)
        raise HTTPException(status_code=500, detail="Training failed. Check server logs.")

    if not version:
        return {"status": "skipped", "reason": "insufficient data or training failed"}

    # Reload the newly trained model
    load_model()

    return {
        "status": "ok",
        "version": version,
        "sample_count": len(request.delay_logs),
    }
