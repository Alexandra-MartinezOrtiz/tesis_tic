#!/bin/bash
set -e

create_database() {
  local database="$1"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
SELECT 'CREATE DATABASE ${database}'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '${database}')\gexec
EOSQL
}

run_scripts() {
  local database="$1"
  for script in /scripts/"$database"/*.sql; do
    echo "Running $script on $database"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -d "$database" -f "$script"
  done
}

create_database ticfis_identity
create_database ticfis_propuestas
create_database ticfis_reportes

run_scripts ticfis_identity
run_scripts ticfis_propuestas
run_scripts ticfis_reportes
