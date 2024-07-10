#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    /*
     * Create application user roles.
     */
    CREATE ROLE app_public_data_api WITH LOGIN PASSWORD 'password';
    CREATE ROLE app_public_data_processor WITH LOGIN PASSWORD 'password';
    CREATE ROLE app_admin WITH LOGIN PASSWORD 'password';
    CREATE ROLE app_publisher WITH LOGIN PASSWORD 'password';

    /*
     * Grant the app_public_data_api role privileges to create new objects which it will own on the public schema.
     */
    GRANT CREATE ON SCHEMA public TO app_public_data_api;

    /*
     * Grant the other application user roles privileges to look up objects on the public schema.
     * Additional privileges are granted to these application user roles by the initial migration.
     */
    GRANT USAGE ON SCHEMA public TO app_public_data_processor;
    GRANT USAGE ON SCHEMA public TO app_admin;
    GRANT USAGE ON SCHEMA public TO app_publisher;
EOSQL
