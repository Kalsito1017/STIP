-- Seed data loaded from Sofia GTFS static feed
-- Run after loading static GTFS into the database.

-- Example: Insert sample routes
INSERT INTO routes (route_id, short_name, long_name, route_type) VALUES
    ('r-204', '204', 'Gotse Delchev – Orlov Most', 3),
    ('r-94',  '94',  'Studentski Grad – Sofia University', 3),
    ('r-1',   '1',   'Sofia University – Mladost 1', 1),
    ('r-2',   '2',   'Obelya – Vitosha', 1)
ON CONFLICT (route_id) DO NOTHING;

-- Example: Insert sample stops (PostGIS geography points)
INSERT INTO stops (stop_id, stop_name, location) VALUES
    ('s-001', 'Orlov Most',          ST_GeogFromText('POINT(23.3342 42.6897)')),
    ('s-002', 'Sofia University',     ST_GeogFromText('POINT(23.3451 42.6939)')),
    ('s-003', 'NDK',                  ST_GeogFromText('POINT(23.3186 42.6871)')),
    ('s-004', 'Serdika',              ST_GeogFromText('POINT(23.3219 42.6977)')),
    ('s-005', 'Central Station',       ST_GeogFromText('POINT(23.3216 42.7104)')),
    ('s-006', 'Mladost 1',            ST_GeogFromText('POINT(23.3782 42.6564)'))
ON CONFLICT (stop_id) DO NOTHING;
