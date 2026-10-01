# GitHub Copilot SDK — demos .NET

Tres recorridos preparados para la sesión:

1. **Museum Exhibit Studio** — demo principal y progresiva.
2. **Accessibility Reviewer** — plan B ya ensayado.
3. **BYOK con Microsoft Foundry** — consola independiente con Microsoft Entra.

Adaptados del
[workshop oficial](https://github.com/github/copilot-sdk-workshop/tree/716bdaaf629817606873b4e22d551e900985c92c).
Ver `THIRD-PARTY-NOTICES.txt`.

## Demo principal: Museum Exhibit Studio

```powershell
dotnet restore .\museum\MuseumDemo.csproj --locked-mode
Set-Location .\museum
npm ci --no-audit --no-fund
Set-Location ..
dotnet build .\museum\MuseumDemo.csproj --no-restore
dotnet run --no-build --project .\museum\MuseumDemo.csproj -- --self-test
dotnet run --no-build --project .\museum\MuseumDemo.csproj -- --preflight
```

Ejecutar una etapa:

```powershell
dotnet run --no-build --project .\museum\MuseumDemo.csproj -- --stage 01 --model gpt-5.4-mini
```

Cambiar `01` por:

| Etapa | Archivo a mostrar | Resultado observable |
|---|---|---|
| 01 | `museum\Stages\01-FirstSession.cs` | Cliente, runtime, sesión y respuesta final. |
| 02 | `museum\Stages\02-Streaming.cs` | Respuesta incremental. |
| 03 | `museum\Stages\03-CuratorVoice.cs` | System prompt y voz del curador. |
| 04 | `museum\Stages\04-ApprovedFacts.cs` | Tool local `approved_fact_lookup`. |
| 05 | `museum\Stages\05-ProveStructure.cs` | Validación determinista PASS/FAIL. |
| 06 | `museum\Stages\06-WikipediaResearch.cs` | Sesión MCP separada, allowlist y fuentes. |
| 99 | `museum\Stages\99-Finished.cs` | Investigación, generación grounded y validación. |

Ensayo completo:

```powershell
.\Rehearse.ps1
```

La investigación de Wikipedia nunca modifica los hechos aprobados usados para
generar la exhibición.

## Plan B: Accessibility Reviewer

El proyecto original continúa disponible en la raíz:

```powershell
dotnet restore .\AccessibilityDemo.csproj --locked-mode
npm ci --no-audit --no-fund
dotnet build .\AccessibilityDemo.csproj --no-restore
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --self-test
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --preflight
```

Para las etapas 05, 06 y 99, mantener la página controlada en otro terminal:

```powershell
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --serve
```

Ejecutar una etapa o el ensayo:

```powershell
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --stage 05 --model gpt-5.4-mini
.\Rehearse-Accessibility.ps1
```

## Tercera demo: BYOK con Microsoft Foundry

La consola `byok` usa `ProviderConfig`, Responses API y un token Microsoft Entra
obtenido mediante Azure CLI. No usa una API key ni imprime el token.

Configurar en un terminal no compartido:

```powershell
$env:FOUNDRY_RESOURCE_URL = 'https://<resource>.openai.azure.com'
$env:FOUNDRY_MODEL = '<deployment-name>'
```

Preparar y ejecutar:

```powershell
dotnet restore .\byok\ByokConsole.csproj --locked-mode
dotnet build .\byok\ByokConsole.csproj --no-restore
dotnet run --no-build --project .\byok\ByokConsole.csproj -- --preflight
dotnet run --no-build --project .\byok\ByokConsole.csproj -- --run
```

Consultar [`byok\README.md`](byok/README.md) antes del directo.

## Otros lenguajes y streaming

- Primeras sesiones mínimas: [`examples`](examples/README.md).
- Guía complementaria: [Streaming de una respuesta — español](docs/02-streaming-es.md).

## Límites

- Los outputs de modelos varían: comprobar eventos, tools y marcadores de
  validación, no prometer texto exacto.
- Un system prompt guía; no autoriza.
- Una allowlist o permiso no equivale a sandbox.
- Las comprobaciones estructurales no prueban exactitud factual.
- No guardar endpoints privados, tokens, claves ni salidas de ensayo en el repo.
