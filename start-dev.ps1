# ============================================================
#  TIC-FIS — Levantar todo el entorno de desarrollo
#  Ejecutar desde la raiz del proyecto: .\start-dev.ps1
# ============================================================

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition

Write-Host "1/6  Iniciando PostgreSQL (Docker)..." -ForegroundColor Cyan
Push-Location "$root\backend"
docker-compose up -d
Pop-Location

Write-Host "2/6  Identity Service  -> http://localhost:5001" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command",
  "cd '$root\backend\services\identity-service'; dotnet run --project Identity.Api"

Write-Host "3/6  Propuestas Service -> http://localhost:5002" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command",
  "cd '$root\backend\services\propuestas-service'; dotnet run --project Propuestas.Api"

Write-Host "4/6  Reportes Service  -> http://localhost:5003" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command",
  "cd '$root\backend\services\reportes-service'; dotnet run --project Reportes.Api"

Write-Host "5/6  API Gateway       -> http://localhost:5000" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command",
  "cd '$root\backend\gateway\TicFis.ApiGateway'; dotnet run"

Write-Host "6/6  Frontend Angular  -> http://localhost:4200" -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command",
  "cd '$root\frontend\tic-fis-web'; npm start"

Write-Host ""
Write-Host "Servicios iniciando en ventanas separadas." -ForegroundColor Green
Write-Host "Espera ~30s y abre: http://localhost:4200" -ForegroundColor Green
