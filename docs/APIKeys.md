# API Keys & Secrets — Procurement Checklist

> **Reminder:** None of these keys should ever be committed to source control.
> Use environment variables, .NET User Secrets (`dotnet user-secrets`), or a vault.

---

## Mandatory (Blockers)

### 1. Sofia GTFS Static Feed
| Field | Detail |
|-------|--------|
| **Description** | Static GTFS zip containing routes, stops, trips, stop_times, calendar |
| **Where to get it** | Sofia Municipality open data portal / Център за градска мобилност (CGM) |
| **Expected URL format** | `https://.../sofia_gtfs_static.zip` |
| **Env var** | `GTFS_STATIC_URL` |
| **Used by** | Worker (seed job), seed scripts |

### 2. Sofia GTFS Realtime Feed
| Field | Detail |
|-------|--------|
| **Description** | GTFS-RT VehiclePositions protobuf feed — live positions of all vehicles |
| **Where to get it** | CGM / Sofia Traffic API (may require registration or API key in URL) |
| **Expected URL format** | `https://.../VehiclePositions.pb?key=YOUR_KEY` |
| **Env var** | `GTFS_RT_FEED_URL` |
| **Used by** | Worker (GTFSPollingJob every 15s) |

### 3. Database Password
| Field | Detail |
|-------|--------|
| **Description** | PostgreSQL superuser or app-user password |
| **Where to get it** | Self-generated (e.g. `openssl rand -base64 32`) |
| **Env var** | `DB_CONNECTION_STRING` |
| **Format** | `Host=postgres;Database=sofia_transport;Username=stip_app;Password=XXXXXX` |
| **Used by** | API, Worker, ML service |

---

## Optional / Future

### 4. Weather API Key
| Field | Detail |
|-------|--------|
| **Description** | Historical + forecast weather data to feed the `weather_score` ML feature |
| **Where to get it** | [OpenWeatherMap](https://openweathermap.org/api) (free tier: 1,000 calls/day) or [WeatherAPI.com](https://www.weatherapi.com/) |
| **Env var** | `WEATHER_API_KEY` |
| **Used by** | ML service (feature engineering) |

### 5. JWT Signing Key
| Field | Detail |
|-------|--------|
| **Description** | Secret key for signing JWT access/refresh tokens (if admin auth is implemented) |
| **Where to get it** | Self-generated (`openssl rand -base64 64`) |
| **Env var** | `JWT_SECRET_KEY` |
| **Used by** | API (auth middleware) |

---

## Quick-Start: Set Secrets for Local Dev

```bash
# .NET User Secrets (API project)
cd backend/SofiaTransport.Api
dotnet user-secrets init
dotnet user-secrets set "GTFS_RT_FEED_URL" "https://..."
dotnet user-secrets set "GTFS_STATIC_URL" "https://..."
dotnet user-secrets set "DB_CONNECTION_STRING" "Host=localhost;..."

# Or create a .env file in the project root (gitignored)
cp .env.example .env
# Fill in the values
```
