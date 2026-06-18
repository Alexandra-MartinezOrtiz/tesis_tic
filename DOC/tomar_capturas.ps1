# =============================================================
# ROBOT DE CAPTURAS - TIC-FIS
# Toma capturas de pantalla de todas las páginas del sistema
# Guarda en DOC\capturas\ con fecha y hora
#
# REQUISITOS: Todos los servicios deben estar corriendo
#   - Identity  : http://localhost:5001
#   - Propuestas: http://localhost:5002
#   - Reportes  : http://localhost:5003
#   - Frontend  : http://localhost:4200
#
# USO: Click derecho en este archivo → "Ejecutar con PowerShell"
#      O desde terminal: .\tomar_capturas.ps1
# =============================================================

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# --- Configuración ---
$carpeta = "$PSScriptRoot\capturas\$(Get-Date -Format 'yyyy-MM-dd_HH-mm')"
$espera  = 4   # segundos que espera antes de capturar cada página

New-Item -ItemType Directory -Path $carpeta -Force | Out-Null
Write-Host "Capturas se guardarán en: $carpeta" -ForegroundColor Cyan

# --- Función para capturar pantalla completa ---
function Capturar {
    param([string]$nombre)
    Start-Sleep -Seconds $espera
    $bounds  = [System.Windows.Forms.Screen]::PrimaryScreen.Bounds
    $bitmap  = New-Object System.Drawing.Bitmap($bounds.Width, $bounds.Height)
    $graphic = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphic.CopyFromScreen($bounds.Location, [System.Drawing.Point]::Empty, $bounds.Size)
    $ruta = "$carpeta\$nombre.png"
    $bitmap.Save($ruta)
    $graphic.Dispose()
    $bitmap.Dispose()
    Write-Host "  Guardado: $nombre.png" -ForegroundColor Green
}

# --- Función para abrir URL y capturar ---
function AbrirYCapturar {
    param([string]$url, [string]$nombre, [int]$espera_extra = 0)
    Write-Host "Abriendo: $url" -ForegroundColor Yellow
    Start-Process $url
    if ($espera_extra -gt 0) { Start-Sleep -Seconds $espera_extra }
    Capturar $nombre
}

# =========================================================
# 1. FRONTEND - Login
# =========================================================
Write-Host "`n[1/8] Frontend - Login" -ForegroundColor Magenta
AbrirYCapturar "http://localhost:4200/login" "01_frontend_login" 2

# =========================================================
# 2. FRONTEND - Dashboard / Propuestas
# =========================================================
Write-Host "`n[2/8] Frontend - Propuestas" -ForegroundColor Magenta

# Hacer login automático para obtener token
try {
    $body = '{"email":"admin@ticfis.local","password":"Admin123!"}'
    $resp = Invoke-RestMethod -Uri "http://localhost:5001/api/auth/login" -Method POST -Body $body -ContentType "application/json"
    Write-Host "  Token obtenido OK" -ForegroundColor Green
} catch {
    Write-Host "  AVISO: No se pudo obtener token. El frontend pedirá login manual." -ForegroundColor Red
}

AbrirYCapturar "http://localhost:4200/propuestas" "02_frontend_propuestas" 2

# =========================================================
# 3. FRONTEND - Reportes
# =========================================================
Write-Host "`n[3/8] Frontend - Reportes" -ForegroundColor Magenta
AbrirYCapturar "http://localhost:4200/reportes" "03_frontend_reportes" 2

# =========================================================
# 4. FRONTEND - Usuarios
# =========================================================
Write-Host "`n[4/8] Frontend - Usuarios" -ForegroundColor Magenta
AbrirYCapturar "http://localhost:4200/usuarios" "04_frontend_usuarios" 2

# =========================================================
# 5. SWAGGER - Identity Service
# =========================================================
Write-Host "`n[5/8] Swagger - Identity (5001)" -ForegroundColor Magenta
AbrirYCapturar "http://localhost:5001/swagger" "05_swagger_identity" 3

# =========================================================
# 6. SWAGGER - Propuestas Service
# =========================================================
Write-Host "`n[6/8] Swagger - Propuestas (5002)" -ForegroundColor Magenta
AbrirYCapturar "http://localhost:5002/swagger" "06_swagger_propuestas" 3

# =========================================================
# 7. SWAGGER - Reportes Service
# =========================================================
Write-Host "`n[7/8] Swagger - Reportes (5003)" -ForegroundColor Magenta
AbrirYCapturar "http://localhost:5003/swagger" "07_swagger_reportes" 3

# =========================================================
# 8. Verificar servicios (resumen de salud)
# =========================================================
Write-Host "`n[8/8] Verificando estado de servicios..." -ForegroundColor Magenta
$servicios = @(
    @{ nombre = "Identity";   url = "http://localhost:5001/health" },
    @{ nombre = "Propuestas"; url = "http://localhost:5002/health" },
    @{ nombre = "Reportes";   url = "http://localhost:5003/health" },
    @{ nombre = "Gateway";    url = "http://localhost:5000/health" }
)

$resumen = @()
foreach ($svc in $servicios) {
    try {
        $r = Invoke-RestMethod -Uri $svc.url -TimeoutSec 3
        $resumen += "$($svc.nombre): OK"
        Write-Host "  $($svc.nombre): OK" -ForegroundColor Green
    } catch {
        $resumen += "$($svc.nombre): NO RESPONDE"
        Write-Host "  $($svc.nombre): NO RESPONDE" -ForegroundColor Red
    }
}

# Guardar resumen en texto
$fecha = Get-Date -Format "yyyy-MM-dd HH:mm"
$resumen_texto = @"
RESUMEN DE CAPTURAS - TIC-FIS
Fecha: $fecha
===============================
$($resumen -join "`n")
===============================
Capturas guardadas en: $carpeta
"@
$resumen_texto | Out-File "$carpeta\00_resumen.txt" -Encoding utf8
Write-Host "`nResumen guardado en 00_resumen.txt" -ForegroundColor Cyan

# =========================================================
# FIN
# =========================================================
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host " CAPTURAS COMPLETADAS" -ForegroundColor Cyan
Write-Host " Carpeta: $carpeta" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "`nPresiona cualquier tecla para abrir la carpeta..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Start-Process explorer.exe $carpeta
