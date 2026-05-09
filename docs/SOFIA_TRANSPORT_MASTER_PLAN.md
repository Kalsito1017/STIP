# Sofia Transport Intelligence Platform — Master Technical Plan

---

## 1. High-Level Vision

A production-grade, full-stack platform that ingests Sofia GTFS data in real time,
stores and processes it, runs ML predictions, exposes a clean REST API, and
renders a React dashboard. Three layers talk to each other:

```
[GTFS Feed / Transport API]
          │
          ▼
[ASP.NET Core – Ingestion + API]  ◄──►  [PostgreSQL + PostGIS]
          │                                       │
          ▼                                       ▼
[Python ML Service]              ◄──  [Delay Logs / History]
          │
          ▼
[React Frontend – Map + Dashboard]
```

---

## 2. Repository Layout (Monorepo)

```
sofia-transport/
│
├── backend/
│   ├── SofiaTransport.Api/            # ASP.NET Core Web API (entry point)
│   ├── SofiaTransport.Application/    # Use-cases, CQRS handlers, DTOs
│   ├── SofiaTransport.Domain/         # Entities, value objects, domain events
│   ├── SofiaTransport.Infrastructure/ # EF Core, PostGIS, GTFS client, SignalR hub
│   └── SofiaTransport.Worker/         # Background service – GTFS polling
│
├── ml/
│   ├── data/                          # Raw & processed GTFS data
│   ├── notebooks/                     # Exploratory analysis
│   ├── models/                        # Saved model artefacts (.pkl / .joblib)
│   ├── train.py                       # Training pipeline
│   ├── predict.py                     # Inference server (FastAPI)
│   └── requirements.txt
│
├── frontend/
│   ├── src/
│   │   ├── components/                # Reusable UI
│   │   ├── pages/                     # Dashboard, Map, Lines, Stops, Predictions
│   │   ├── hooks/                     # useRealtime, useDelays, usePrediction
│   │   ├── services/                  # Axios API clients
│   │   └── store/                     # Zustand global state
│   └── package.json
│
├── docker/
│   ├── docker-compose.yml
│   ├── Dockerfile.api
│   ├── Dockerfile.worker
│   └── Dockerfile.ml
│
├── database/
│   ├── migrations/                    # EF Core migrations
│   └── seed/                         # Static GTFS seed scripts
│
└── docs/
    ├── architecture.md
    └── api-spec.yaml                  # OpenAPI 3.0
```

---

## 3. Domain Model

### Core Entities

```
Route          { RouteId, ShortName, LongName, Type(Bus|Tram|Metro|Trolley) }
Stop           { StopId, Name, Lat, Lon, Geometry(PostGIS Point) }
Trip           { TripId, RouteId, ServiceDays, Direction }
StopTime       { TripId, StopId, ArrivalTime(scheduled), StopSequence }
Vehicle        { VehicleId, RouteId, TripId, Lat, Lon, Bearing, Speed, Timestamp }
DelayLog       { Id, VehicleId, StopId, TripId, ScheduledArrival,
                 ActualArrival, DelaySeconds, RecordedAt }
ReliabilityScore { RouteId, Date, OnTimePercent, AvgDelaySeconds,
                   Score, PeakHourScore }
```

### Value Objects
- `Coordinates(Lat, Lon)` — immutable, validates range for Sofia area
- `TransitType` — enum: Bus, Tram, Metro, Trolley
- `DelayBucket` — OnTime(<60s), Slight(1-3min), Moderate(3-7min), Severe(>7min)

---

## 4. Backend – ASP.NET Core Clean Architecture

### Layer Responsibilities

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, business rules, domain events. Zero dependencies. |
| **Application** | CQRS with MediatR. Use-cases. No EF references here. |
| **Infrastructure** | EF Core + PostGIS, GTFS HTTP client, Redis cache, SignalR hub |
| **API** | Controllers, middleware, auth, Swagger |
| **Worker** | IHostedService polling GTFS every 15s |

### Key Packages
- `MediatR` — CQRS
- `FluentValidation` — command validation
- `NetTopologySuite` + `Npgsql.EntityFrameworkCore.PostgreSQL` — PostGIS geometry
- `Quartz.NET` — scheduled jobs (delay log aggregation, ML retrain trigger)
- `StackExchange.Redis` — cache live vehicle positions
- `Microsoft.AspNetCore.SignalR` — push live positions to frontend
- `Polly` — resilience for GTFS feed calls

### API Endpoints

```
# Real-time
GET  /api/vehicles/live                    # All live vehicle positions
GET  /api/vehicles/live?routeId=204        # Filtered by route
WS   /hubs/vehicles                        # SignalR – live push

# Lines / Routes
GET  /api/routes                           # All routes
GET  /api/routes/{id}                      # Route detail
GET  /api/routes/{id}/reliability          # Reliability score + history
GET  /api/routes/{id}/delay-pattern        # Avg delay by hour of day

# Stops
GET  /api/stops/{id}                       # Stop info
GET  /api/stops/{id}/predicted-arrivals    # ML-predicted arrivals
GET  /api/stops/{id}/congestion            # Hourly congestion

# Analytics
GET  /api/analytics/heatmap/delays         # GeoJSON heatmap data
GET  /api/analytics/reliability/ranking    # Best/worst routes
GET  /api/analytics/peak-hours             # System-wide peak analysis

# Predictions (proxied from Python ML service)
POST /api/predictions/delay                # { routeId, stopId, scheduledTime }
POST /api/predictions/travel-time          # { fromStop, toStop, departureTime }
```

### Background Worker (SofiaTransport.Worker)

```
GTFSPollingJob (every 15s):
  1. Fetch GTFS-RT feed (VehiclePositions protobuf)
  2. Deserialize → VehiclePosition[]
  3. For each vehicle:
     a. Upsert into Redis (key: vehicle:{id}) — TTL 60s
     b. Broadcast via SignalR
     c. Match to scheduled StopTime
     d. Calculate delay, write DelayLog to Postgres

DelayAggregationJob (every 1h, via Quartz):
  1. Aggregate DelayLogs → ReliabilityScores per route per day
  2. Update rolling 7-day / 30-day averages

MLRetrainTriggerJob (daily at 2:00 AM):
  1. Export last 30 days of DelayLogs to CSV
  2. POST /internal/retrain on Python ML service
```

---

## 5. Database Schema (PostgreSQL + PostGIS)

```sql
-- Enable extensions
CREATE EXTENSION postgis;
CREATE EXTENSION btree_gist;

-- Core GTFS tables (loaded from static feed)
CREATE TABLE routes (
    route_id TEXT PRIMARY KEY,
    short_name TEXT NOT NULL,
    long_name TEXT,
    route_type SMALLINT  -- 0=Tram,1=Metro,3=Bus,11=Trolley
);

CREATE TABLE stops (
    stop_id TEXT PRIMARY KEY,
    stop_name TEXT NOT NULL,
    location GEOGRAPHY(POINT, 4326) NOT NULL  -- PostGIS!
);
CREATE INDEX idx_stops_location ON stops USING GIST(location);

CREATE TABLE trips (
    trip_id TEXT PRIMARY KEY,
    route_id TEXT REFERENCES routes(route_id),
    service_id TEXT,
    direction_id SMALLINT
);

CREATE TABLE stop_times (
    trip_id TEXT REFERENCES trips(trip_id),
    stop_id TEXT REFERENCES stops(stop_id),
    arrival_time INTERVAL,
    stop_sequence INT,
    PRIMARY KEY (trip_id, stop_sequence)
);

-- Live tracking
CREATE TABLE vehicles (
    vehicle_id TEXT PRIMARY KEY,
    route_id TEXT,
    trip_id TEXT,
    location GEOGRAPHY(POINT, 4326),
    bearing FLOAT,
    speed FLOAT,
    recorded_at TIMESTAMPTZ DEFAULT now()
);

-- Analytics tables
CREATE TABLE delay_logs (
    id BIGSERIAL PRIMARY KEY,
    vehicle_id TEXT,
    stop_id TEXT,
    trip_id TEXT,
    route_id TEXT,
    scheduled_arrival TIMESTAMPTZ,
    actual_arrival TIMESTAMPTZ,
    delay_seconds INT GENERATED ALWAYS AS
        (EXTRACT(EPOCH FROM (actual_arrival - scheduled_arrival))::INT) STORED,
    recorded_at TIMESTAMPTZ DEFAULT now()
);
CREATE INDEX idx_delay_logs_route ON delay_logs(route_id, recorded_at DESC);
CREATE INDEX idx_delay_logs_stop  ON delay_logs(stop_id, recorded_at DESC);

CREATE TABLE reliability_scores (
    route_id TEXT,
    score_date DATE,
    on_time_pct FLOAT,
    avg_delay_seconds FLOAT,
    reliability_score FLOAT,  -- on_time_pct - (avg_delay_seconds / 60 * penalty)
    peak_score FLOAT,
    sample_count INT,
    PRIMARY KEY (route_id, score_date)
);
```

---

## 6. Python ML Service

### Stack
- **FastAPI** — expose `/predict` and `/retrain` endpoints
- **pandas** — data wrangling
- **scikit-learn** — baseline Linear Regression + feature pipeline
- **XGBoost** — production model
- **joblib** — model serialisation
- **schedule** / Quartz trigger — retrain pipeline

### Feature Engineering

```python
features = [
    "hour_of_day",          # 0-23
    "day_of_week",          # 0=Mon … 6=Sun
    "is_peak_hour",         # 07-09 | 17-19
    "is_weekend",
    "route_id_encoded",     # label encoded
    "stop_sequence",        # position on route
    "historical_avg_delay", # rolling 7-day avg for this route+hour
    "weather_score",        # optional future: rain/snow flag
]
target = "delay_seconds"
```

### Model Pipeline

```
Train:
  1. Load delay_logs (last 90 days) from Postgres
  2. Feature engineering → DataFrame
  3. Train/val split (80/20, time-based)
  4. Train XGBoostRegressor
  5. Evaluate: MAE, RMSE, R²
  6. Save model → models/xgb_delay_v{version}.joblib
  7. Save metadata → models/model_meta.json

Predict endpoint:
  POST /predict
  { route_id, stop_id, hour, day_of_week, stop_sequence }
  → { predicted_delay_seconds, confidence_interval, model_version }
```

### Reliability Score Formula

```
reliability_score = (on_time_pct * 100) - (avg_delay_minutes * PENALTY_FACTOR)

where:
  on_time_pct      = trips arriving within 60 seconds of schedule
  PENALTY_FACTOR   = 5  (tunable)
  Score range: 0-100, higher = more reliable
```

---

## 7. Frontend – React

### Tech Stack
- **Vite** + React 18
- **TypeScript** throughout
- **Zustand** — global state (vehicles, filters)
- **React Query (TanStack)** — server state, caching, polling
- **Leaflet + react-leaflet** — map
- **Recharts** — delay charts, reliability graphs
- **Tailwind CSS** + **shadcn/ui** — UI components
- **SignalR client** (`@microsoft/signalr`) — real-time vehicle updates

### Pages & Components

```
/map              → LiveMapPage
  ├── VehicleLayer          (SignalR-fed markers)
  ├── StopLayer             (clickable stops)
  ├── DelayHeatmapLayer     (GeoJSON overlay)
  └── FilterPanel           (route type, line number)

/dashboard        → DashboardPage
  ├── SystemOverviewCard    (live count, avg delay right now)
  ├── ReliabilityRanking    (best/worst 10 lines)
  ├── PeakHourChart         (hour vs avg delay, all lines)
  └── DelayTrendChart       (7-day rolling)

/routes/:id       → RouteDetailPage
  ├── RouteHeader           (name, type, score badge)
  ├── DelayByHourChart
  ├── StopDelayBreakdown    (table: each stop's avg delay)
  └── PredictPanel          (enter time → predicted delay)

/stops/:id        → StopDetailPage
  ├── StopMap               (mini map, surrounding stops)
  ├── PredictedArrivals     (next 5 arrivals w/ predictions)
  └── CongestionHeatmap     (hour × day heatmap grid)
```

### Real-time Strategy

```
Vehicle positions:  SignalR  → Zustand store → Leaflet markers
                    (no polling – pure push)

Delay heatmap:      React Query, refetch every 60s
Predictions:        On-demand POST (user triggers)
Reliability scores: React Query, staleTime = 5min
```

---

## 8. Infrastructure & DevOps

### Docker Compose (local dev)

```yaml
services:
  postgres:   image: postgis/postgis:16-3.4
  redis:      image: redis:7-alpine
  api:        build: ./docker/Dockerfile.api    port: 5000
  worker:     build: ./docker/Dockerfile.worker
  ml:         build: ./docker/Dockerfile.ml     port: 8000
  frontend:   build: ./frontend                 port: 3000
```

### Environment Variables (key ones)

```
GTFS_RT_FEED_URL=https://...          # Sofia GTFS realtime URL
GTFS_STATIC_URL=https://...           # Static GTFS zip
DB_CONNECTION_STRING=Host=postgres;...
REDIS_CONNECTION=redis:6379
ML_SERVICE_URL=http://ml:8000
POLL_INTERVAL_SECONDS=15
```

---

## 9. Development Phases

### Phase 1 — Foundation (Week 1-2)
- [ ] Repo setup, Docker Compose, PostgreSQL with PostGIS
- [ ] Load static GTFS data (routes, stops, stop_times)
- [ ] ASP.NET Core project with Clean Architecture layers
- [ ] Basic CRUD endpoints: routes, stops
- [ ] React app scaffold + Leaflet map showing stops

### Phase 2 — Real-Time (Week 3)
- [ ] GTFS-RT polling Worker
- [ ] Redis vehicle cache
- [ ] SignalR hub
- [ ] Live vehicle markers on map
- [ ] DelayLog writing pipeline

### Phase 3 — Analytics (Week 4)
- [ ] Delay aggregation jobs (Quartz)
- [ ] ReliabilityScore computation
- [ ] Analytics endpoints (heatmap, ranking, peak hours)
- [ ] Dashboard page + Recharts visualisations

### Phase 4 — ML (Week 5)
- [ ] Python FastAPI service scaffold
- [ ] Data export from Postgres
- [ ] Feature engineering + XGBoost training
- [ ] `/predict` endpoint live
- [ ] ASP.NET proxy + React prediction UI

### Phase 5 — Polish (Week 6)
- [ ] OpenAPI spec, Swagger UI
- [ ] Error handling, retry policies (Polly)
- [ ] Unit tests (domain + application layers)
- [ ] Performance: DB indices, Redis caching audit
- [ ] README + architecture diagram

---

## 10. Key Technical Decisions & Rationale

| Decision | Choice | Why |
|---|---|---|
| ORM | EF Core + NetTopologySuite | PostGIS geometry support, migrations |
| Realtime | SignalR over WebSocket polling | Lower latency, server push |
| Cache | Redis | Vehicle positions expire fast — TTL fits perfectly |
| State mgmt | Zustand | Simpler than Redux for this domain |
| ML serving | FastAPI sidecar | Decoupled from .NET; Python ML ecosystem |
| Scheduler | Quartz.NET | Production-grade, persistent jobs |
| Map | Leaflet | Lighter than Mapbox, no API key needed |
| Charts | Recharts | React-native, composable, good TS support |

---

## 11. What Makes This Stand Out vs "Just a Bus Tracker"

1. **Reliability Score** — own invented metric, rankable, time-series trackable
2. **Predictive arrivals** — ML-backed, not just schedule + current delay
3. **Delay heatmap** — spatial intelligence via PostGIS
4. **Peak hour pattern analysis** — actionable insight per route
5. **Clean Architecture** — shows seniority in backend design
6. **Event-driven real-time** — SignalR + Redis, not naive polling
7. **Separated ML service** — microservice thinking

---

*This document is the single source of truth before any code is written.*
*Each phase maps to a focused, deliverable milestone.*
