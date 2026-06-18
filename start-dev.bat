@echo off
echo ============================================================
echo  TIC-FIS - Iniciando entorno de desarrollo
echo ============================================================

set ROOT=%~dp0

echo [1/6] PostgreSQL (Docker)...
cd /d "%ROOT%backend"
docker-compose up -d

echo [2/6] Identity Service  ^> http://localhost:5001
start "Identity Service" powershell -NoExit -Command "cd '%ROOT%backend\services\identity-service'; dotnet run --project Identity.Api"

echo [3/6] Propuestas Service ^> http://localhost:5002
start "Propuestas Service" powershell -NoExit -Command "cd '%ROOT%backend\services\propuestas-service'; dotnet run --project Propuestas.Api"

echo [4/6] Reportes Service  ^> http://localhost:5003
start "Reportes Service" powershell -NoExit -Command "cd '%ROOT%backend\services\reportes-service'; dotnet run --project Reportes.Api"

echo [5/6] API Gateway       ^> http://localhost:5000
start "API Gateway" powershell -NoExit -Command "cd '%ROOT%backend\gateway\TicFis.ApiGateway'; dotnet run"

echo [6/6] Frontend Angular  ^> http://localhost:4200
start "Frontend Angular" cmd /k "cd /d %ROOT%frontend\tic-fis-web && npm start"

echo.
echo Todos los servicios iniciando en ventanas separadas.
echo Espera ~30 segundos y abre: http://localhost:4200
echo.
pause
