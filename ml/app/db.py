"""Shared database utilities for the STIP ML service."""

import os
import logging

logger = logging.getLogger(__name__)


def parse_db_connection_string(conn_str: str) -> dict:
    """Convert .NET-style connection string to psycopg2 kwargs.

    Input:  'Host=postgres;Database=sofia_transport;Username=stip_app;Password=secret'
    Output: {'host': 'postgres', 'dbname': 'sofia_transport', 'user': 'stip_app', 'password': 'secret'}
    """
    mapping = {
        "Host": "host",
        "Database": "dbname",
        "Username": "user",
        "Password": "password",
        "Port": "port",
    }
    params: dict[str, str] = {}
    for part in conn_str.split(";"):
        if "=" in part:
            key, value = part.split("=", 1)
            key, value = key.strip(), value.strip()
            if key in mapping:
                params[mapping[key]] = value
    return params


def get_connection_params() -> dict:
    """Parse DB_CONNECTION_STRING env var into psycopg2 kwargs."""
    conn_str = os.environ.get("DB_CONNECTION_STRING", "")
    if not conn_str:
        return {}
    return parse_db_connection_string(conn_str)


def create_connection():
    """Create a psycopg2 connection from DB_CONNECTION_STRING env var."""
    import psycopg2

    params = get_connection_params()
    if not params:
        raise RuntimeError("DB_CONNECTION_STRING environment variable is not set")
    return psycopg2.connect(**params)
