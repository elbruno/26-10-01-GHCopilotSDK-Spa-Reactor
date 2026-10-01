# Ensayo automatizado de Museum.
# Enseña como repetir etapas del SDK con el mismo modelo y guardar evidencia
# local del directo; puede consumir cuota porque ejecuta prompts reales.
param(
    [string]$Model = 'gpt-5.4-mini',
    [ValidateSet('01', '02', '03', '04', '05', '06', '99')]
    [string[]]$Stages = @('01', '02', '03', '04', '05', '06', '99')
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'museum\MuseumDemo.csproj'
$logs = Join-Path $PSScriptRoot '..\preparation\museum-rehearsal'
New-Item -ItemType Directory -Force $logs | Out-Null
foreach ($stage in $Stages) {
    # Cada etapa conserva su salida para comparar streaming, tools, permisos y validacion.
    Write-Host "Ensayo Museum $stage (puede consumir cuota). Wikipedia MCP se usa en 06 y 99."
    & dotnet run --no-build --project $project -- --stage $stage --model $Model 2>&1 |
        Tee-Object (Join-Path $logs "$stage.txt")
    if ($LASTEXITCODE -ne 0) {
        throw "La etapa $stage fallo. No se considera ensayada."
    }
}
