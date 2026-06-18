# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TIC-FIS** is a microservices-based web system for managing Curriculum Integration Work (TIC) proposals at the Faculty of Systems Engineering. It handles the full lifecycle: creation, review, committee approval/rejection, and reporting.

## Architecture

Three independent microservices, each with its own PostgreSQL database, fronted by an API Gateway:

| Service | Port | Database | Responsibility |
|---------|------|----------|----------------|
| Identity | 5001 | `ticfis_identity` | Auth, JWT, users, roles, permissions |
| Propuestas | 5002 | `ticfis_propuestas` | Proposal lifecycle, state machine, reviews |
| Reportes | 5003 | `ticfis_reportes` | Queries, filters, PDF export |
| API Gateway | 5000 | — | Single entry point, routing, CORS |

Each backend service follows **Clean Architecture** with four layers:
- `<Service>.Api` — Controllers, DI wiring, Swagger config
- `<Service>.Application` — Use cases, DTOs, service interfaces
- `<Service>.Domain` — Entities, domain rules, interfaces
- `<Service>.Infrastructure` — EF Core DbContext, repository implementations

The frontend is Angular 17 (standalone components) with feature-based modules under `src/app/features/`, shared utilities in `src/app/core/`, and lazy-loaded routes.

Design patterns in use: Repository, Unit of Work, CQRS, Mediator, Strategy, Specification.

## Common Commands

### Frontend (`frontend/tic-fis-web/`)
```bash
npm install          # Install dependencies
npm start            # Dev server at localhost:4200
npm run build        # Production build → dist/tic-fis-web/
```

### Backend (from solution root or individual project directories)
```bash
dotnet restore                          # Restore NuGet packages
dotnet build                            # Build entire solution
dotnet run --project backend/<Service>/<Service>.Api  # Run a specific service
dotnet publish -c Release -o ./publish  # Publish for deployment
```

### Database & Docker
```bash
docker-compose up -d     # Start PostgreSQL 16 (port 5432)
docker-compose down      # Stop containers
```

Database init SQL files are in `docker/postgres/sql/` and run automatically on first container start.

## Key Configuration

**Backend** (`appsettings.json` per service):
- `ConnectionStrings:DefaultConnection` — PostgreSQL connection string
- `Jwt:SigningKey`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays`

**Frontend** (`src/app/core/constants/`): API base URLs and endpoint constants.

Default local credentials: `Host=localhost;Port=5432;Username=postgres;Password=postgres`.

## Proposal State Machine

Proposals (`propuestas`) transition through these states: `Borrador` → `En Revisión` → `Pendiente Aprobación` → `Aprobada` / `Rechazada`. State transitions are recorded in `propuesta_historial_estados`. Business rules governing transitions live in `Propuestas.Domain`.

## Architecture Documentation

Detailed architecture decisions, database schemas, API endpoint specs, and field mappings are documented in `DOC/arquitectura_tic_fis_modulos_abc.md`. Consult it before adding new endpoints or changing the data model.
