#!/bin/bash
set -e
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
CREATE DATABASE ticfis_identity;
CREATE DATABASE ticfis_propuestas;
CREATE DATABASE ticfis_reportes;
EOSQL
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -d ticfis_identity -f /scripts/ticfis_identity/001_init.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -d ticfis_propuestas -f /scripts/ticfis_propuestas/001_init.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" -d ticfis_reportes -f /scripts/ticfis_reportes/001_init.sql
