-- Sofia Transport Intelligence Platform — Database Schema
-- PostgreSQL + PostGIS

CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS btree_gist;

-- Routes
CREATE TABLE IF NOT EXISTS routes (
    route_id   TEXT PRIMARY KEY,
    short_name TEXT NOT NULL,
    long_name  TEXT,
    route_type SMALLINT NOT NULL
);

-- Stops
CREATE TABLE IF NOT EXISTS stops (
    stop_id   TEXT PRIMARY KEY,
    stop_name TEXT NOT NULL,
    location  GEOGRAPHY(POINT, 4326) NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_stops_location ON stops USING GIST(location);

-- Trips
CREATE TABLE IF NOT EXISTS trips (
    trip_id     TEXT PRIMARY KEY,
    route_id    TEXT REFERENCES routes(route_id),
    service_id  TEXT,
    direction_id SMALLINT NOT NULL DEFAULT 0
);

-- Stop Times
CREATE TABLE IF NOT EXISTS stop_times (
    trip_id       TEXT REFERENCES trips(trip_id),
    stop_id       TEXT REFERENCES stops(stop_id),
    arrival_time  INTERVAL,
    stop_sequence INT NOT NULL,
    PRIMARY KEY (trip_id, stop_sequence)
);

-- Vehicles (live tracking)
CREATE TABLE IF NOT EXISTS vehicles (
    vehicle_id  TEXT PRIMARY KEY,
    route_id    TEXT,
    trip_id     TEXT,
    location    GEOGRAPHY(POINT, 4326),
    bearing     FLOAT DEFAULT 0,
    speed       FLOAT DEFAULT 0,
    recorded_at TIMESTAMPTZ DEFAULT now()
);

-- Delay Logs (analytics)
CREATE TABLE IF NOT EXISTS delay_logs (
    id                BIGSERIAL PRIMARY KEY,
    vehicle_id        TEXT,
    stop_id           TEXT,
    trip_id           TEXT,
    route_id          TEXT,
    scheduled_arrival TIMESTAMPTZ,
    actual_arrival    TIMESTAMPTZ,
    delay_seconds     INT GENERATED ALWAYS AS
        (EXTRACT(EPOCH FROM (actual_arrival - scheduled_arrival))::INT) STORED,
    recorded_at       TIMESTAMPTZ DEFAULT now()
);
CREATE INDEX IF NOT EXISTS idx_delay_logs_route ON delay_logs(route_id, recorded_at DESC);
CREATE INDEX IF NOT EXISTS idx_delay_logs_stop  ON delay_logs(stop_id, recorded_at DESC);

-- Reliability Scores
CREATE TABLE IF NOT EXISTS reliability_scores (
    route_id           TEXT,
    score_date         DATE,
    on_time_pct        FLOAT,
    avg_delay_seconds  FLOAT,
    reliability_score  FLOAT,
    peak_score         FLOAT,
    sample_count       INT DEFAULT 0,
    PRIMARY KEY (route_id, score_date)
);
