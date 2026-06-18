# =====================================================
# INICIAR TODO - TIC-FIS
# Levanta los 4 servicios backend + frontend Angular
# =====================================================

Write-Host "Iniciando TIC-FIS..." -ForegroundColor Cyan

# Matar procesos anteriores en los puertos si existen
$puertos = @(5000, 5001, 5002, 5003)
foreach ($puerto in $puertos) {
    $pids = netstat -ano | Select-String ":$puerto\s.*LISTENING" | ForEach-Object {
        ($_ -split '\s+')[-1]
    }
    foreach ($p in $pids) {
        if ($p -match '^\d+$') {
            Stop-Process -Id $p -Force -ErrorAction SilentlyContinue
        }
    }
}
Start-Sleep -Seconds 1

# Iniciar los 4 backends
Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" `
    -ArgumentList "run" `
    -WorkingDirectory "c:\ESPACIO_DE_TRABAJO\TESIS\backend\services\identity-service\Identity.Api" `
    -WindowStyle Minimized

Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" `
    -ArgumentList "run" `
    -WorkingDirectory "c:\ESPACIO_DE_TRABAJO\TESIS\backend\services\propuestas-service\Propuestas.Api" `
    -WindowStyle Minimized

Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" `
    -ArgumentList "run" `
    -WorkingDirectory "c:\ESPACIO_DE_TRABAJO\TESIS\backend\services\reportes-service\Reportes.Api" `
    -WindowStyle Minimized

Start-Process -FilePath "C:\Program Files\dotnet\dotnet.exe" `
    -ArgumentList "run" `
    -WorkingDirectory "c:\ESPACIO_DE_TRABAJO\TESIS\backend\gateway\TicFis.ApiGateway" `
    -WindowStyle Minimized

Write-Host "Backends iniciados (compilando en segundo plano...)" -ForegroundColor Green

# Iniciar frontend
Start-Process -FilePath "cmd.exe" `
    -ArgumentList "/c npm start" `
    -WorkingDirectory "c:\ESPACIO_DE_TRABAJO\TESIS\frontend\tic-fis-web" `
    -WindowStyle Minimized

Write-Host "Frontend iniciado." -ForegroundColor Green
Write-Host ""
Write-Host "Servicios:" -ForegroundColor Cyan
Write-Host "  Frontend  -> http://localhost:4200" -ForegroundColor White
Write-Host "  Gateway   -> http://localhost:5000" -ForegroundColor White
Write-Host "  Identity  -> http://localhost:5001/swagger" -ForegroundColor White
Write-Host "  Propuestas-> http://localhost:5002/swagger" -ForegroundColor White
Write-Host "  Reportes  -> http://localhost:5003/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Esperando ~20s para que compilen..." -ForegroundColor Yellow
Start-Sleep -Seconds 20

# Verificar puertos
$activos = netstat -ano | findstr "LISTENING" | findstr ":5000 :5001 :5002 :5003 :4200"
if ($activos) {
    Write-Host "Sistema listo!" -ForegroundColor Green
    $activos
} else {
    Write-Host "Aun compilando, espera un poco mas..." -ForegroundColor Yellow
}
