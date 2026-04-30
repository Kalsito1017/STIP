# Package Manifest

Every dependency used across the monorepo, grouped by layer and ecosystem.

---

## .NET — NuGet Packages

### `SofiaTransport.Api` (Presentation Layer)

| Package | Purpose |
|---------|---------|
| `MediatR` | CQRS — dispatch commands/queries |
| `FluentValidation.AspNetCore` | Request validation pipeline |
| `Swashbuckle.AspNetCore` | OpenAPI 3.0 / Swagger UI |
| `Serilog.AspNetCore` | Structured logging |
| `Serilog.Sinks.Console` | Console sink for structured logs |
| `Serilog.Sinks.PostgreSQL` | Ship logs to Postgres (optional) |
| `Microsoft.AspNetCore.SignalR` | WebSocket hub for live vehicle push |
| `Microsoft.AspNetCore.ResponseCompression` | Brotli/Gzip compression |
| `Microsoft.AspNetCore.Cors` | CORS policy middleware |
| `AspNetCoreRateLimit` | Rate limiting middleware |

### `SofiaTransport.Application` (Use-Case Layer)

| Package | Purpose |
|---------|---------|
| `MediatR` | CQRS contracts |
| `FluentValidation` | Command/query DTO validation |
| `AutoMapper` | Entity ↔ DTO mapping |
| `Microsoft.Extensions.Logging.Abstractions` | Logging interface |

### `SofiaTransport.Domain` (Core Layer)

| Package | Purpose |
|---------|---------|
| *(none — zero-dependency core)* | Entities, value objects, domain events |

### `SofiaTransport.Infrastructure` (Data & External Services)

| Package | Purpose |
|---------|---------|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | EF Core provider for PostgreSQL |
| `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` | PostGIS geometry support via NTS |
| `NetTopologySuite` | Spatial types (Point, Polygon, etc.) |
| `Microsoft.EntityFrameworkCore.Design` | Migrations tooling |
| `StackExchange.Redis` | Cache live vehicle positions |
| `Quartz.Extensions.Hosting` | Cron-based scheduling (aggregation, retrain trigger) |
| `Quartz.Serialization.Json` | JSON job store for Quartz |
| `Microsoft.AspNetCore.SignalR` | SignalR hub definition |
| `Google.Protobuf` | GTFS-RT protobuf deserialization |
| `Polly` | Retry / circuit-breaker for GTFS feed HTTP calls |
| `Polly.Extensions.Http` | Polly integration with `IHttpClientFactory` |
| `Microsoft.Extensions.Http.Polly` | HttpClient with Polly policies |

### `SofiaTransport.Worker` (Background Service)

| Package | Purpose |
|---------|---------|
| `Microsoft.Extensions.Hosting` | `IHostedService` / `BackgroundService` |
| `StackExchange.Redis` | Write vehicle positions to Redis |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Write DelayLogs |
| `Google.Protobuf` | Deserialize GTFS-RT |
| `Polly` | Resilience on feed calls |
| `Quartz.Extensions.Hosting` | If Worker runs Quartz jobs directly |

---

## Python — pip Packages

| Package | Version Lock | Purpose |
|---------|-------------|---------|
| `fastapi` | ≥0.110 | REST API for ML inference |
| `uvicorn[standard]` | ≥0.27 | ASGI server |
| `pandas` | ≥2.2 | DataFrame wrangling |
| `numpy` | ≥1.26 | Numerical ops |
| `scikit-learn` | ≥1.4 | Baseline LinearRegression |
| `xgboost` | ≥2.0 | Production delay-prediction model |
| `joblib` | ≥1.3 | Model serialization (`.joblib`) |
| `httpx` | ≥0.27 | Async HTTP client (to .NET API & Postgres) |
| `psycopg2-binary` | ≥2.9 | PostgreSQL connector for training data export |
| `schedule` | ≥1.2 | Simple retrain-scheduling (dev); Quartz trigger in prod |
| `pydantic` | ≥2.6 | Request/response validation (FastAPI built-in) |
| `python-dotenv` | ≥1.0 | Load `.env` for local dev |

---

## Frontend — npm Packages

### Core

| Package | Purpose |
|---------|---------|
| `react` `react-dom` | UI library (v18+) |
| `typescript` | Type safety |
| `vite` | Build tool & dev server |
| `@vitejs/plugin-react` | Vite React plugin |

### State & Data

| Package | Purpose |
|---------|---------|
| `zustand` | Global client state (vehicles, filters) |
| `@tanstack/react-query` | Server state, caching, refetch |
| `axios` | HTTP client for REST API |

### Map

| Package | Purpose |
|---------|---------|
| `leaflet` | Interactive map with free OSM tiles |
| `react-leaflet` | React bindings for Leaflet |
| `@types/leaflet` | TypeScript types |

### Charts

| Package | Purpose |
|---------|---------|
| `recharts` | Delay, reliability, peak-hour charts |

### Real-Time

| Package | Purpose |
|---------|---------|
| `@microsoft/signalr` | SignalR WebSocket client |

### UI / Styling

| Package | Purpose |
|---------|---------|
| `tailwindcss` | Utility-first CSS |
| `postcss` `autoprefixer` | Tailwind toolchain |
| `@radix-ui/react-*` | Headless UI primitives (shadcn/ui deps) |
| `class-variance-authority` | Component variant API |
| `clsx` `tailwind-merge` | Class merging utilities |
| `lucide-react` | Icon library |

### Dev Dependencies

| Package | Purpose |
|---------|---------|
| `eslint` + plugins | Linting |
| `prettier` | Formatting |
| `@types/react` `@types/react-dom` | React type defs |

---

## Docker Images

| Image | Tag | Purpose |
|-------|-----|---------|
| `postgis/postgis` | `16-3.4` | PostgreSQL + PostGIS database |
| `redis` | `7-alpine` | In-memory cache |
| `python` | `3.11-slim` | ML service runtime |
| `mcr.microsoft.com/dotnet/aspnet` | `8.0` | API & Worker runtime |
| `mcr.microsoft.com/dotnet/sdk` | `8.0` | Build stage (multi-stage) |
| `node` | `20-alpine` | Frontend build stage |

---

## Dev Tools (Host Machine)

| Tool | Min Version | Purpose |
|------|------------|---------|
| .NET SDK | 8.0 | Build + run C# projects |
| Node.js | 20 LTS | Run Vite dev server, npm |
| Python | 3.11 | Run ML service locally |
| Docker + Compose | v2.24+ | Containerized dev environment |
| `dotnet-ef` | 8.x | EF Core migrations CLI |
| `openssl` | — | Generate secrets |
