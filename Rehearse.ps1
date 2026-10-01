param(
    [string]$Model = 'gpt-5.4-mini',
    [ValidateSet('01', '02', '03', '04', '05', '06', '99')]
    [string[]]$Stages = @('01', '02', '03', '04', '05', '06', '99')
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'AccessibilityDemo.csproj'
$logs = Join-Path $PSScriptRoot '.runs\rehearsal'
New-Item -ItemType Directory -Force $logs | Out-Null
foreach ($stage in $Stages) {
    Write-Host "Ensayo real $stage (puede consumir cuota). Pagina local requerida para 05, 06 y 99."
    & dotnet run --no-build --project $project -- --stage $stage --model $Model 2>&1 |
        Tee-Object (Join-Path $logs "$stage.txt")
    if ($LASTEXITCODE -ne 0) {
        throw "La etapa $stage fallo. No se considera ensayada."
    }
}
