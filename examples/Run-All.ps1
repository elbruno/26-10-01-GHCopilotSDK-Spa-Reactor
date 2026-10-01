param(
    [switch]$SkipSetup,
    [switch]$SetupOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$examplesRoot = $PSScriptRoot

if ($SkipSetup -and $SetupOnly) {
    throw '-SkipSetup y -SetupOnly no se pueden usar juntos.'
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory)]
        [string]$Label,
        [Parameter(Mandatory)]
        [scriptblock]$Command
    )

    Write-Host ''
    Write-Host ('=' * 72)
    Write-Host $Label
    Write-Host ('=' * 72)
    & $Command
    if ($LASTEXITCODE -ne 0) {
        throw "$Label terminó con código $LASTEXITCODE."
    }
}

foreach ($command in @('copilot', 'dotnet', 'python', 'go', 'node', 'npm')) {
    if (-not (Get-Command $command -ErrorAction SilentlyContinue)) {
        throw "$command no está disponible."
    }
}

if (-not $SkipSetup) {
    Invoke-Checked 'Preparando C#' {
        & dotnet restore (Join-Path $examplesRoot 'csharp\CopilotSdkHello.csproj') --locked-mode
    }

    $pythonRoot = Join-Path $examplesRoot 'python'
    $pythonExecutable = Join-Path $pythonRoot '.venv\Scripts\python.exe'
    if (-not (Test-Path -LiteralPath $pythonExecutable)) {
        Invoke-Checked 'Creando el entorno virtual de Python' {
            & python -m venv (Join-Path $pythonRoot '.venv')
        }
    }
    Invoke-Checked 'Preparando Python' {
        & $pythonExecutable -m pip install --disable-pip-version-check -e $pythonRoot
    }

    Invoke-Checked 'Preparando Go' {
        & go -C (Join-Path $examplesRoot 'go') mod download
    }

    Invoke-Checked 'Preparando TypeScript' {
        & npm --prefix (Join-Path $examplesRoot 'typescript') ci --no-audit --no-fund
    }
}

if ($SetupOnly) {
    Write-Host ''
    Write-Host 'Los cuatro ejemplos quedaron preparados; no se llamó al modelo.'
    return
}

Invoke-Checked 'C#' {
    & dotnet run --no-restore --project (Join-Path $examplesRoot 'csharp\CopilotSdkHello.csproj')
}

$pythonExecutable = Join-Path $examplesRoot 'python\.venv\Scripts\python.exe'
if (-not (Test-Path -LiteralPath $pythonExecutable)) {
    throw 'Falta examples\python\.venv. Ejecuta Run-All.ps1 sin -SkipSetup.'
}
Invoke-Checked 'Python' {
    Push-Location (Join-Path $examplesRoot 'python')
    try {
        & $pythonExecutable .\main.py
    }
    finally {
        Pop-Location
    }
}

Invoke-Checked 'Go' {
    & go -C (Join-Path $examplesRoot 'go') run .
}

Invoke-Checked 'TypeScript' {
    & npm --prefix (Join-Path $examplesRoot 'typescript') start
}

Write-Host ''
Write-Host 'Los cuatro ejemplos finalizaron correctamente.'
