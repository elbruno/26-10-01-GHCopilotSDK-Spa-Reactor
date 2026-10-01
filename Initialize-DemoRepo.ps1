param(
    [switch]$SkipPreflight
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Require-Command([string]$Name, [string]$InstallHint) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name no esta disponible. $InstallHint"
    }
}

Require-Command 'dotnet' 'Instala .NET 10 SDK desde https://dotnet.microsoft.com/download/dotnet/10.0.'
Require-Command 'node' 'Instala Node.js 22 o superior desde https://nodejs.org/.'
Require-Command 'copilot' 'Instala GitHub Copilot CLI y autentica la cuenta fuera de camara.'

$project = Join-Path $PSScriptRoot 'AccessibilityDemo.csproj'

Write-Host 'Restaurando paquetes .NET...'
& dotnet restore $project --locked-mode
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore fallo.' }

Write-Host 'Instalando dependencias locales de Playwright MCP...'
& npm ci --prefix $PSScriptRoot --no-audit --no-fund
if ($LASTEXITCODE -ne 0) { throw 'npm ci fallo.' }

Write-Host 'Compilando la demo...'
& dotnet build $project --no-restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet build fallo.' }

Write-Host 'Ejecutando comprobaciones locales...'
& dotnet run --no-build --project $project -- --self-test
if ($LASTEXITCODE -ne 0) { throw 'self-test fallo.' }

if (-not $SkipPreflight) {
    Write-Host 'Comprobando autenticacion y modelos disponibles...'
    & dotnet run --no-build --project $project -- --preflight
    if ($LASTEXITCODE -ne 0) {
        throw 'preflight fallo. Autentica Copilot fuera de camara y vuelve a ejecutar este script.'
    }
}

Write-Host ''
Write-Host 'Repositorio preparado. Para la pagina controlada:'
Write-Host '  dotnet run --no-build --project .\AccessibilityDemo.csproj -- --serve'
Write-Host 'Para una etapa:'
Write-Host '  dotnet run --no-build --project .\AccessibilityDemo.csproj -- --stage 01 --model gpt-5.4-mini'
