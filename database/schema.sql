-- Sofia Transport Intelligence Platform — Database Schema
-- PostgreSQL + PostGIS

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS btree_gist;

-- Routes
CREATE TABLE IF NOT EXISTS routes (
    route_id   TEXT PRIMARY KEY,
    short_name TEXT NOT NULL,
    long_name  TEXT,
    route_type SMALLINT NOT NULL,
    CONSTRAINT chk_routes_type CHECK (route_type IN (0,1,2,3,4,5,6,7,11,12))
);
CREATE INDEX IF NOT EXISTS idx_routes_type ON routes(route_type);

-- Stops
CREATE TABLE IF NOT EXISTS stops (
    stop_id   TEXT PRIMARY KEY,
    stop_name TEXT NOT NULL,
    location  GEOGRAPHY(POINT, 4326) NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_stops_location ON stops USING GIST(location);
CREATE INDEX IF NOT EXISTS idx_stops_name ON stops(stop_name);

-- Trips
CREATE TABLE IF NOT EXISTS trips (
    trip_id     TEXT PRIMARY KEY,
    route_id    TEXT REFERENCES routes(route_id),
    service_id  TEXT,
    direction_id SMALLINT NOT NULL DEFAULT 0
);

-- Stop Times
CREATE TABLE IF NOT EXISTS stop_times (
    trip_id       TEXT REFERENCES trips(trip_id) ON DELETE CASCADE,
    stop_id       TEXT REFERENCES stops(stop_id) ON DELETE CASCADE,
    arrival_time  INTERVAL,
    departure_time INTERVAL,
    stop_sequence INT NOT NULL,
    PRIMARY KEY (trip_id, stop_sequence)
);
CREATE INDEX IF NOT EXISTS idx_stop_times_stop_arrival ON stop_times(stop_id, arrival_time);
CREATE INDEX IF NOT EXISTS idx_stop_times_trip_stop ON stop_times(trip_id, stop_id);

-- Vehicles (live tracking)
CREATE TABLE IF NOT EXISTS vehicles (
    vehicle_id  TEXT PRIMARY KEY,
    route_id    TEXT REFERENCES routes(route_id) ON DELETE SET NULL,
    trip_id     TEXT REFERENCES trips(trip_id) ON DELETE SET NULL,
    location    GEOGRAPHY(POINT, 4326),
    bearing     FLOAT DEFAULT 0,
    speed       FLOAT DEFAULT 0,
    recorded_at TIMESTAMPTZ DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_vehicles_recorded_at ON vehicles(recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_vehicles_route_id ON vehicles(route_id, recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_vehicles_trip_id ON vehicles(trip_id);
CREATE INDEX IF NOT EXISTS idx_vehicles_location ON vehicles USING GIST(location);

-- Immutable helper: EXTRACT(HOUR FROM timestamptz) is STABLE (timezone-dependent).
-- Wrapping it as an IMMUTABLE function allows use in index expressions.
CREATE OR REPLACE FUNCTION hour_of_day(TIMESTAMPTZ)
RETURNS INTEGER
IMMUTABLE PARALLEL SAFE
AS $$ SELECT EXTRACT(HOUR FROM $1 AT TIME ZONE 'UTC')::INTEGER $$
LANGUAGE SQL;

-- Delay Logs (analytics)
-- delay_seconds is calculated by the application (GtfsPollingService) and may be NULL when delay cannot be determined
CREATE TABLE IF NOT EXISTS delay_logs (
    id                BIGSERIAL PRIMARY KEY,
    vehicle_id        TEXT REFERENCES vehicles(vehicle_id) ON DELETE SET NULL,
    stop_id           TEXT REFERENCES stops(stop_id) ON DELETE SET NULL,
    trip_id           TEXT REFERENCES trips(trip_id) ON DELETE SET NULL,
    route_id          TEXT REFERENCES routes(route_id) ON DELETE SET NULL,
    scheduled_arrival TIMESTAMPTZ,
    actual_arrival    TIMESTAMPTZ,
    delay_seconds     INT,  -- application-calculated, nullable
    delay_bucket      SMALLINT DEFAULT 0, -- 0=OnTime, 1=Slight, 2=Moderate, 3=Severe
    recorded_at       TIMESTAMPTZ DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_delay_logs_route ON delay_logs(route_id, recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_delay_logs_stop  ON delay_logs(stop_id, recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_delay_logs_recorded_at ON delay_logs(recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_delay_logs_vehicle ON delay_logs(vehicle_id, recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_delay_logs_route_hour ON delay_logs(route_id, hour_of_day(scheduled_arrival), recorded_at DESC) WHERE delay_seconds IS NOT NULL;

-- Reliability Scores
CREATE TABLE IF NOT EXISTS reliability_scores (
    route_id           TEXT REFERENCES routes(route_id) ON DELETE CASCADE,
    score_date         DATE,
    on_time_pct        FLOAT,
    avg_delay_seconds  FLOAT,
    reliability_score  FLOAT,
    peak_score         FLOAT,
    sample_count       INT DEFAULT 0,
    PRIMARY KEY (route_id, score_date)
);

-- Users (authentication)
CREATE TABLE IF NOT EXISTS users (
    id            UUID PRIMARY KEY,
    email         TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    full_name     TEXT NOT NULL,
    created_at    TIMESTAMPTZ NOT NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON users(email);

-- Shapes (route geometry)
CREATE TABLE IF NOT EXISTS shapes (
    id       BIGSERIAL PRIMARY KEY,
    route_id TEXT NOT NULL REFERENCES routes(route_id) ON DELETE CASCADE,
    sequence INT NOT NULL,
    lat      DOUBLE PRECISION NOT NULL,
    lon      DOUBLE PRECISION NOT NULL,
    UNIQUE (route_id, sequence)
);
CREATE INDEX IF NOT EXISTS idx_shapes_route_sequence ON shapes(route_id, sequence);
CREATE INDEX IF NOT EXISTS idx_shapes_location ON shapes USING GIST(ST_SetSRID(ST_MakePoint(lon, lat), 4326));

-- Materialized view: pre-aggregated delay stats per route per hour
CREATE MATERIALIZED VIEW IF NOT EXISTS mv_hourly_delays AS
SELECT
    route_id,
    EXTRACT(HOUR FROM scheduled_arrival)::INT AS hour_of_day,
    COUNT(*) AS sample_count,
    AVG(delay_seconds)::FLOAT AS avg_delay_seconds,
    STDDEV(delay_seconds)::FLOAT AS stddev_delay_seconds,
    SUM(CASE WHEN ABS(delay_seconds) <= 60 THEN 1 ELSE 0 END)::FLOAT / COUNT(*) AS on_time_pct
FROM delay_logs
WHERE route_id IS NOT NULL AND delay_seconds IS NOT NULL
GROUP BY route_id, EXTRACT(HOUR FROM scheduled_arrival);

CREATE UNIQUE INDEX IF NOT EXISTS idx_mv_hourly_delays ON mv_hourly_delays(route_id, hour_of_day);

-- Function to refresh materialized view concurrently
CREATE OR REPLACE FUNCTION refresh_hourly_delays()
RETURNS void AS $$
BEGIN
    REFRESH MATERIALIZED VIEW CONCURRENTLY mv_hourly_delays;
END;
$$ LANGUAGE plpgsql;

-- Helper function for spatial nearest-stop lookup
CREATE OR REPLACE FUNCTION find_nearest_stop(lat FLOAT, lon FLOAT, radius_meters FLOAT DEFAULT 200)
RETURNS TABLE(stop_id TEXT, stop_name TEXT, distance_meters FLOAT) AS $$
BEGIN
    RETURN QUERY
    SELECT
        s.stop_id,
        s.stop_name,
        ST_Distance(s.location, ST_SetSRID(ST_MakePoint(lon, lat), 4326)::geography)::FLOAT AS distance_meters
    FROM stops s
    WHERE ST_DWithin(s.location, ST_SetSRID(ST_MakePoint(lon, lat), 4326)::geography, radius_meters)
    ORDER BY distance_meters
    LIMIT 1;
END;
$$ LANGUAGE plpgsql STABLE;
