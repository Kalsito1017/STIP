# Sofia Transport Intelligence Platform (STIP)

A full-stack, real-time public transport analytics platform for **Sofia, Bulgaria** — tracking vehicles live, computing reliability scores, predicting delays with machine learning, and surfacing trip updates and service alerts.

## Contents

- [Sofia Transport Intelligence Platform (STIP)](#sofia-transport-intelligence-platform-stip)
  - [Contents](#contents)
  - [Architecture](#architecture)
    - [Docker Services](#docker-services)
  - [Key Features](#key-features)
  - [Tech Stack](#tech-stack)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Quick Start](#quick-start)
    - [Access Points](#access-points)
    - [Local Development](#local-development)
    - [Environment Variables](#environment-variables)
  - [Project Structure](#project-structure)

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
├── backend/
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
├── graphify-out/                      # Knowledge graph (code structure & cross-module relationships)
├── SOFIA_TRANSPORT_MASTER_PLAN.md     # Full technical specification
├── Idea.md                            # Project concept & feature vision
└── HighLevelVision.md                 # Architecture diagram
```

---
