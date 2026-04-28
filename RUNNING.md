# How to Run STIP (All at Once)

## One-Time Setup

```bash
cp .env.example .env
```

Edit `.env` — fill in `DB_PASSWORD`, `GTFS_RT_FEED_URL`, `GTFS_STATIC_URL`.

---

## Start Everything

```bash
  docker compose --env-file .env -f docker/docker-compose.yml up -d --build
```

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5173 |
| API + Swagger | http://localhost:5000/swagger |
| ML Service | http://localhost:8000/docs |
| pgAdmin | http://localhost:5050 |

---

## Stop

```bash
docker compose --env-file .env -f docker/docker-compose.yml down
```
