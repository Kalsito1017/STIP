-- Migration: AddUserFavorites
-- Run this against the sofia_transport database

START TRANSACTION;

CREATE TABLE IF NOT EXISTS user_favorites (
    id bigserial NOT NULL,
    user_id uuid NOT NULL,
    entity_type character varying(20) NOT NULL,
    entity_id character varying(50) NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT now(),
    CONSTRAINT pk_user_favorites PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_user_favorites_unique
    ON user_favorites (user_id, entity_type, entity_id);

CREATE INDEX IF NOT EXISTS idx_user_favorites_user
    ON user_favorites (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260507194746_AddUserFavorites', '10.0.2')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;
