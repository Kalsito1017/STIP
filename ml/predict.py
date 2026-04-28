import os
import json
from datetime import datetime
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import joblib
import pandas as pd
import numpy as np

app = FastAPI(title="STIP ML Service", version="1.0.0")

MODEL_DIR = os.path.join(os.path.dirname(__file__), "models")
META_PATH = os.path.join(MODEL_DIR, "model_meta.json")
MODEL_PATH = os.path.join(MODEL_DIR, "xgb_delay_v1.joblib")

_model = None
_model_meta = {}


def load_model():
    global _model, _model_meta
    if os.path.exists(MODEL_PATH):
        _model = joblib.load(MODEL_PATH)
    if os.path.exists(META_PATH):
        with open(META_PATH) as f:
            _model_meta = json.load(f)


load_model()


class PredictionRequest(BaseModel):
    route_id: str
    stop_id: str
    hour: int
    day_of_week: int  # 0=Mon .. 6=Sun
    stop_sequence: int


class PredictionResponse(BaseModel):
    predicted_delay_seconds: float
    confidence_interval: list[float]
    model_version: str


class RetrainRequest(BaseModel):
    delay_logs: list[dict]


@app.get("/health")
def health():
    return {"status": "ok", "model_loaded": _model is not None}


@app.post("/predict", response_model=PredictionResponse)
def predict(request: PredictionRequest):
    if _model is None:
        raise HTTPException(status_code=503, detail="Model not loaded")

    features = pd.DataFrame([[
        request.hour,
        request.day_of_week,
        1 if request.hour in [7, 8, 9, 17, 18, 19] else 0,
        1 if request.day_of_week >= 5 else 0,
        hash(request.route_id) % 1000,
        request.stop_sequence,
        0,
        0,
    ]], columns=[
        "hour_of_day", "day_of_week", "is_peak_hour", "is_weekend",
        "route_id_encoded", "stop_sequence", "historical_avg_delay", "weather_score"
    ])

    pred = float(_model.predict(features)[0])
    return PredictionResponse(
        predicted_delay_seconds=max(0, pred),
        confidence_interval=[max(0, pred - 30), pred + 30],
        model_version=_model_meta.get("version", "v1"),
    )


@app.post("/internal/retrain")
def retrain(request: RetrainRequest):
    if not request.delay_logs:
        return {"status": "skipped", "reason": "no data"}

    df = pd.DataFrame(request.delay_logs)
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
        return {"status": "skipped", "reason": "insufficient data (need >= 10 rows)"}

    from xgboost import XGBRegressor
    from sklearn.model_selection import train_test_split

    X_train, X_val, y_train, y_val = train_test_split(X, y, test_size=0.2, random_state=42)
    model = XGBRegressor(n_estimators=100, max_depth=5, learning_rate=0.1)
    model.fit(X_train, y_train)

    os.makedirs(MODEL_DIR, exist_ok=True)
    joblib.dump(model, MODEL_PATH)

    version = f"v{datetime.utcnow().strftime('%Y%m%d%H%M%S')}"
    with open(META_PATH, "w") as f:
        json.dump({
            "version": version,
            "trained_at": datetime.utcnow().isoformat(),
            "feature_count": len(features),
            "sample_count": len(df),
            "mae": float(np.mean(np.abs(y_val - model.predict(X_val)))),
        }, f)

    load_model()
    return {"status": "ok", "version": version, "sample_count": len(df)}
