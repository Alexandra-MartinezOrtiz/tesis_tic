# Requiere: PostgreSQL con BDs creadas (docker compose en backend/), dotnet-ef global.
# Genera modelos desde la BD existente (database-first). Revisa diff antes de commitear.
# Uso: desde backend/: .\tools\scaffold-from-db.ps1

$ErrorActionPreference = "Stop"
$password = "postgres"
$hostName = "localhost"
$port = "5432"
$user = "postgres"

dotnet tool update --global dotnet-ef 2>$null
dotnet tool install --global dotnet-ef 2>$null

$identityCs = "Host=$hostName;Port=$port;Database=ticfis_identity;Username=$user;Password=$password"
dotnet ef dbcontext scaffold $identityCs Npgsql.EntityFrameworkCore.PostgreSQL `
  --project services/identity-service/Identity.Infrastructure `
  --startup-project services/identity-service/Identity.Api `
  --output-dir Persistence/Scaffold/EfGenerated `
  --context TicfisIdentityDbContextEf `
  --namespace Identity.Infrastructure.Persistence.Scaffold.EfGenerated `
  --force --no-onconfiguring

$propCs = "Host=$hostName;Port=$port;Database=ticfis_propuestas;Username=$user;Password=$password"
dotnet ef dbcontext scaffold $propCs Npgsql.EntityFrameworkCore.PostgreSQL `
  --project services/propuestas-service/Propuestas.Infrastructure `
  --startup-project services/propuestas-service/Propuestas.Api `
  --output-dir Persistence/Scaffold/EfGenerated `
  --context TicfisPropuestasDbContextEf `
  --namespace Propuestas.Infrastructure.Persistence.Scaffold.EfGenerated `
  --force --no-onconfiguring

$repCs = "Host=$hostName;Port=$port;Database=ticfis_reportes;Username=$user;Password=$password"
dotnet ef dbcontext scaffold $repCs Npgsql.EntityFrameworkCore.PostgreSQL `
  --project services/reportes-service/Reportes.Infrastructure `
  --startup-project services/reportes-service/Reportes.Api `
  --output-dir Persistence/Scaffold/EfGenerated `
  --context TicfisReportesDbContextEf `
  --namespace Reportes.Infrastructure.Persistence.Scaffold.EfGenerated `
  --force --no-onconfiguring

Write-Host "Scaffold completado en Persistence/Scaffold/EfGenerated por servicio. Fusionar o comparar con Persistence/Scaffolded."
