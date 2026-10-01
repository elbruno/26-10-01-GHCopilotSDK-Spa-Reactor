param(
    [Parameter(Mandatory)]
    [ValidateSet('gpt-6.1-sol', 'gpt-6-luna', 'grok-4.6')]
    [string]$Model,

    [ValidateSet('entra', 'api-key')]
    [string]$AuthMode,

    [string]$ResourceUrl
)

$ErrorActionPreference = 'Stop'

# Ejecutar con dot-sourcing para que las variables queden en el terminal actual:
# . .\byok\Set-ByokDemo.ps1 -Model gpt-6.1-sol
if ([string]::IsNullOrWhiteSpace($AuthMode)) {
    $AuthMode = if ($Model -eq 'grok-4.6') { 'api-key' } else { 'entra' }
}

if ([string]::IsNullOrWhiteSpace($ResourceUrl)) {
    $ResourceUrl = $env:FOUNDRY_RESOURCE_URL
}

if ([string]::IsNullOrWhiteSpace($ResourceUrl)) {
    $ResourceUrl = Read-Host 'Foundry resource URL (no se guarda)'
}

$resource = $null
if (-not [Uri]::TryCreate($ResourceUrl, [UriKind]::Absolute, [ref]$resource) -or
    $resource.Scheme -ne 'https') {
    throw 'La URL de Foundry debe ser HTTPS.'
}

$env:FOUNDRY_RESOURCE_URL = $ResourceUrl.TrimEnd('/')
$env:FOUNDRY_MODEL = $Model
$env:FOUNDRY_AUTH_MODE = $AuthMode

if ($AuthMode -eq 'api-key') {
    if ([string]::IsNullOrWhiteSpace($env:FOUNDRY_API_KEY)) {
        $env:FOUNDRY_API_KEY = Read-Host 'Foundry API key (entrada oculta)' -MaskInput
    }

    if ([string]::IsNullOrWhiteSpace($env:FOUNDRY_API_KEY)) {
        throw 'La API key no puede estar vacia.'
    }
}
else {
    # Evita que una key vieja cambie accidentalmente la demo Entra.
    Remove-Item Env:\FOUNDRY_API_KEY -ErrorAction SilentlyContinue
}

Write-Host "Perfil BYOK listo: model=$Model auth=$AuthMode"
Write-Host 'Endpoint y credenciales no se muestran ni se guardan.'
