# Sofia Transport Intelligence Platform (STIP)

A full-stack, real-time public transport analytics platform for **Sofia, Bulgaria** — tracking vehicles live, computing reliability scores, predicting delays with machine learning, and surfacing trip updates and service alerts.

## Contents

- [Architecture](#architecture)
- [Key Features](#key-features)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Overview](#api-overview)
- [Real-time Feeds](#real-time-feeds)
- [Development Roadmap](#development-roadmap)

---

## Architecture

```
[GTFS Feeds / Transport API]
  ├─ Vehicle Positions ─────────────┐
  ├─ Trip Updates ─────────────────┤
  └─ Service Alerts ───────────────┤
                                    ▼
[ASP.NET Core – Ingestion + API]  ◄──►  [PostgreSQL + PostGIS]
           │                                       │
           ▼                                       ▼
[Python ML Service]              ◄──  [Delay Logs / History]
           │
           ▼
[React Frontend – Map + Dashboard]
```

### Docker Services

| Service | Image / Runtime | Port | Purpose |
|---------|----------------|------|---------|
| **postgres** | postgis/postgis:16-3.4 | 5432 | Spatial database (routes, stops, delays) |
| **redis** | redis:7-alpine | 6379 | Live vehicle, trip update & alert cache |
| **api** | .NET 10 ASP.NET Core | 5000 | REST API + SignalR real-time hub |
| **worker** | .NET 10 | — | GTFS-RT polling (vehicles, trip updates, alerts), delay aggregation, ML retrain trigger |
| **ml** | Python 3.11 FastAPI | 8000 | XGBoost delay prediction service |

---

## Key Features

- **Live Vehicle Tracking** — GTFS realtime positions via SignalR WebSocket push, rendered on a Leaflet map
- **Real-time Trip Updates** — Per-stop arrival/departure delays from GTFS-RT trip updates feed, cached in Redis and pushed via SignalR
- **Service Alerts** — Disruptions, reroutes, and station closures from GTFS-RT alerts feed, displayed as real-time banner notifications
- **Delay Intelligence** — per-route delay patterns by hour, peak-hour analysis, spatial heatmap via PostGIS
- **ML-Powered Predictions** — XGBoost model predicts arrival delays based on route, stop, time of day, and historical patterns
- **Reliability Score System** — custom invented metric ranking routes from best to worst: `(on_time_pct × 100) − (avg_delay_min × 5)`
- **Stop Analytics** — congestion by hour, predicted arrivals, transfer hub identification

---

## Tech Stack

| Layer | Technologies |
|-------|-------------|
| **Backend** | .NET 10, ASP.NET Core, MediatR (CQRS), EF Core + PostGIS, FluentValidation, AutoMapper |
| **Realtime** | SignalR, Redis |
| **Scheduling** | Quartz.NET (cron jobs: delay aggregation, ML retrain) |
| **Resilience** | Polly (retry/circuit-breaker on GTFS HTTP calls) |
| **ML** | Python 3.11, FastAPI, XGBoost, scikit-learn, pandas, joblib |
| **Frontend** | React 19, TypeScript, Vite, Tailwind CSS 4, Leaflet, Recharts |
| **State** | Zustand (global), TanStack React Query (server) |
| **Database** | PostgreSQL 16 + PostGIS 3.4 |
| **DevOps** | Docker Compose, multi-stage Dockerfiles |

---

## Getting Started

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) and Docker Compose
- [.NET SDK 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (for local development)
- [Node.js 20+](https://nodejs.org/) (for frontend dev)
- [Python 3.11+](https://www.python.org/downloads/) (for ML development)

### Quick Start

```bash
# 1. Clone the repository
git clone <repo-url> && cd STIP

# 2. Set up environment variables
cp .env.example .env
# Edit .env with your GTFS feed URLs and credentials

# 3. Start all services
docker compose -f docker/docker-compose.yml up -d

# 4. Verify services
docker compose -f docker/docker-compose.yml ps
```

### Access Points

| Service | URL |
|---------|-----|
| **API + Swagger** | http://localhost:5000/swagger |
| **SignalR Hub** | ws://localhost:5000/hubs/vehicles |
| **Frontend (dev)** | http://localhost:5173 |
| **ML Service** | http://localhost:8000/docs |

### Local Development

```bash
# Backend
cd src
dotnet restore
dotnet run --project SofiaTransport.Api

# Worker (separate terminal)
dotnet run --project SofiaTransport.Worker

# Frontend (separate terminal)
cd frontend
npm install
npm run dev

# ML (separate terminal)
cd ml
pip install -r requirements.txt
uvicorn predict:app --port 8000 --reload
```

### Environment Variables

See `.env.example` for the full list. Key variables:

| Variable | Description |
|----------|-------------|
| `GTFS_STATIC_URL` | Sofia static GTFS feed (routes, stops, schedules) |
| `GTFS_RT_FEED_URL` | Sofia GTFS realtime vehicle positions feed |
| `GTFS_RT_TRIP_UPDATES_URL` | Sofia GTFS realtime trip updates feed (optional) |
| `GTFS_RT_ALERTS_URL` | Sofia GTFS realtime service alerts feed (optional) |
| `DB_CONNECTION_STRING` | PostgreSQL connection (default: `Host=postgres;...`) |
| `REDIS_CONNECTION` | Redis connection (default: `redis:6379`) |
| `ML_SERVICE_URL` | ML service URL (default: `http://ml:8000`) |
| `POLL_INTERVAL_SECONDS` | GTFS polling interval (default: `15`) |

> **Tip:** Trip updates and alerts are optional feeds. Leave `GTFS_RT_TRIP_UPDATES_URL` and `GTFS_RT_ALERTS_URL` empty to disable them.

---

## Project Structure

```
STIP/
├── src/
│   ├── SofiaTransport.Api/            # ASP.NET Core Web API + Swagger
│   ├── SofiaTransport.Application/    # CQRS handlers, DTOs, validation
│   ├── SofiaTransport.Domain/         # Entities, value objects (zero deps)
│   ├── SofiaTransport.Infrastructure/ # EF Core, PostGIS, Redis, SignalR, ML proxy
│   └── SofiaTransport.Worker/         # GTFS polling, Quartz jobs
├── ml/
│   ├── predict.py                     # FastAPI inference server
│   ├── train.py                       # XGBoost training pipeline
│   ├── data/                          # Raw & processed data
│   ├── models/                        # Saved model artifacts (.joblib)
│   └── notebooks/                     # Exploratory analysis
├── frontend/
│   └── src/
│       ├── components/                # Reusable UI components (AlertBanner, TripUpdatesList, StatCard, etc.)
│       ├── pages/                     # Map, Dashboard, Routes, Stops, Analytics
│       ├── hooks/                     # SignalR, queries, predictions
│       ├── services/                  # Axios API clients
│       └── store/                     # Zustand global state
├── docker/
│   ├── docker-compose.yml
│   ├── Dockerfile.api
│   ├── Dockerfile.worker
│   └── Dockerfile.ml
├── database/
│   ├── schema.sql                     # Full DDL (7 tables, PostGIS, indexes)
│   └── seed/init.sql                  # Sample Sofia data
├── tests/
│   ├── SofiaTransport.Domain.Tests/
│   └── SofiaTransport.Tests/
├── docs/
│   ├── Packages.md                    # Complete dependency manifest
│   └── APIKeys.md                     # API key procurement checklist
├── SOFIA_TRANSPORT_MASTER_PLAN.md     # Full technical specification
├── Idea.md                            # Project concept & feature vision
└── HighLevelVision.md                 # Architecture diagram
```

---

## API Overview

| Endpoint | Description |
|----------|-------------|
| `GET /api/vehicles/live` | All live vehicle positions (optional `?routeId=` filter) |
| `GET /api/tripupdates/live` | Live trip updates with per-stop delays (optional `?routeId=` filter) |
| `GET /api/alerts` | Active service alerts (optional `?routeId=` filter) |
| `GET /api/routes` | All routes |
| `GET /api/routes/{id}/reliability` | Reliability score history |
| `GET /api/routes/{id}/delay-pattern` | Average delay by hour of day |
| `GET /api/stops/{id}/predicted-arrivals` | ML-predicted arrival times |
| `GET /api/analytics/heatmap/delays` | GeoJSON delay heatmap |
| `GET /api/analytics/reliability/ranking` | Best/worst routes by reliability |
| `POST /api/predictions/delay` | Predict delay for route/stop/time |
| `WS /hubs/vehicles` | SignalR live push (VehicleUpdated, TripUpdated, AlertUpdated) |

Full interactive docs available at http://localhost:5000/swagger when running.

---

## Real-time Feeds

The platform ingests three optional GTFS-RT protobuf feeds from Sofia's urban mobility center:

| Feed | URL | Data | Redis TTL |
|------|-----|------|-----------|
| **Vehicle Positions** | `GTFS_RT_FEED_URL` | Live vehicle locations, speed, bearing | 120s |
| **Trip Updates** | `GTFS_RT_TRIP_UPDATES_URL` | Per-stop arrival/departure delays, schedule relationships | 120s |
| **Service Alerts** | `GTFS_RT_ALERTS_URL` | Disruptions, reroutes, station closures, cause/effect | 300s |

All three feeds are polled by the Worker service at the configured `POLL_INTERVAL_SECONDS`. Trip updates and alerts are **optional** — the application starts normally without them.

---

## Development Roadmap

| Phase | Focus | Timeline |
|-------|-------|----------|
| **Phase 1** | Docker Compose, PostGIS schema, GTFS static load, Clean Architecture scaffold, React + Leaflet | Weeks 1–2 |
| **Phase 2** | GTFS-RT polling, Redis cache, SignalR hub, live vehicle map, DelayLog pipeline | Week 3 |
| **Phase 3** | Quartz aggregation, ReliabilityScore computation, analytics endpoints, Dashboard + Recharts | Week 4 |
| **Phase 4** | FastAPI ML service, XGBoost training, `/predict` endpoint, React prediction UI | Week 5 |
| **Phase 5** | Swagger, error handling, unit tests, DB indexes, documentation | Week 6 |
| **Phase 6** | GTFS-RT Trip Updates & Alerts feeds, real-time SignalR push, AlertBanner UI, TripUpdatesList | Week 7 |