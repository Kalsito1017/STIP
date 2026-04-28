# STIP Database Layer

This folder contains the PostgreSQL + PostGIS schema, seed data, and documentation for the Sofia Transport Intelligence Platform.

## Schema (`schema.sql`)

Run manually or let Docker init handle it:

```bash
psql -h localhost -U stip_app -d sofia_transport -f database/schema.sql
```

### Tables
- `routes` — transit routes (bus, tram, trolley, metro)
- `stops` — stop locations with PostGIS `GEOGRAPHY(POINT, 4326)`
- `trips` — scheduled trips per route
- `stop_times` — arrival times per trip/stop
- `vehicles` — live vehicle positions
- `delay_logs` — arrival delay analytics (nullable `delay_seconds`)
- `reliability_scores` — daily route reliability metrics

### Materialized View
- `mv_hourly_delays` — pre-aggregated delay stats per route per hour

Refresh concurrently:
```sql
SELECT refresh_hourly_delays();
```

Or manually:
```sql
REFRESH MATERIALIZED VIEW CONCURRENTLY mv_hourly_delays;
```

### Helper Function
- `find_nearest_stop(lat, lon, radius_meters)` — spatial nearest-stop lookup

Example:
```sql
SELECT * FROM find_nearest_stop(42.6897, 23.3342, 200);
```

## Seed Data (`seed/init.sql`)

Load sample Sofia data:

```bash
psql -h localhost -U stip_app -d sofia_transport -f database/seed/init.sql
```

All inserts use `ON CONFLICT DO NOTHING` for safe re-runs.

## EF Core Migrations

Migrations live in `backend/SofiaTransport.Infrastructure/Persistence/Migrations/`.

### Create a new migration
```bash
cd backend
dotnet ef migrations add <MigrationName> \
  --project SofiaTransport.Infrastructure/SofiaTransport.Infrastructure.csproj \
  --startup-project SofiaTransport.Api/SofiaTransport.Api.csproj \
  --context TransportDbContext
```

### Apply migrations
```bash
cd backend
dotnet ef database update \
  --project SofiaTransport.Infrastructure/SofiaTransport.Infrastructure.csproj \
  --startup-project SofiaTransport.Api/SofiaTransport.Api.csproj \
  --context TransportDbContext
```

## Connecting with psql

```bash
psql -h localhost -p 5432 -U stip_app -d sofia_transport
```

Connection string (from API):
```
Host=localhost;Database=sofia_transport;Username=stip_app;Password=<DB_PASSWORD>
```

## Redis Configuration

The Redis container uses `docker/redis.conf` with:
- `maxmemory 256mb`
- `maxmemory-policy allkeys-lru`
- RDB + AOF persistence for durability
- `tcp-keepalive 300`

Vehicle cache TTL is 120 seconds (managed by `RedisVehicleCache.cs`).
