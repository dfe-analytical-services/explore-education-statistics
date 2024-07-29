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
     * Create a public_data_read_write group role which can be granted to user roles requiring read and write privileges on public schema objects.
     */
    CREATE ROLE public_data_read_write WITH NOLOGIN;

    /*
     * Allow the public_data_read_write group role to access objects in the public schema.
     * This does not include the permissions to read (i.e. `SELECT`) or modify (i.e. `INSERT`, `UPDATE`, `DELETE`) the content of those objects.
     */
    GRANT USAGE ON SCHEMA public TO public_data_read_write;

    /*
     * Grant privileges to the public_data_read_write group role for all tables and sequences in the public schema subsequently created by app_public_data_api.
     */
    ALTER DEFAULT PRIVILEGES FOR ROLE app_public_data_api IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE, TRUNCATE, REFERENCES ON TABLES TO public_data_read_write;
    ALTER DEFAULT PRIVILEGES FOR ROLE app_public_data_api IN SCHEMA public GRANT SELECT, UPDATE ON SEQUENCES TO public_data_read_write;

    /*
     * Grant membership of the public_data_read_write group role to the application user roles.
     */
    GRANT public_data_read_write TO app_public_data_api;
    GRANT public_data_read_write TO app_public_data_processor;
    GRANT public_data_read_write TO app_admin;
    GRANT public_data_read_write TO app_publisher;
EOSQL
