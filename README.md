# GitHub Copilot SDK — demos .NET

Cuatro recorridos preparados para la sesión:

1. **Hello worlds** — el mismo patrón en C#, Python, Go, TypeScript y Rust.
2. **Conceptos del SDK en C#** — streaming, system prompt, tool local,
   permisos, MCP, skills y WorkIQ seguro.
3. **Museum Exhibit Studio** — demo principal de la segunda mitad.
4. **Accessibility Reviewer** — plan B ya ensayado.

BYOK con Microsoft Foundry queda preparado como bloque opcional.

Adaptados del
[workshop oficial](https://github.com/github/copilot-sdk-workshop/tree/716bdaaf629817606873b4e22d551e900985c92c).
Ver `THIRD-PARTY-NOTICES.txt`.

## Primera mitad: conceptos del SDK en C#

```powershell
dotnet restore .\csharp-sdk-concepts\CsharpSdkConcepts.csproj --locked-mode
dotnet build .\csharp-sdk-concepts\CsharpSdkConcepts.csproj --no-restore
dotnet run --no-build --project .\csharp-sdk-concepts\CsharpSdkConcepts.csproj -- --self-test
dotnet run --no-build --project .\csharp-sdk-concepts\CsharpSdkConcepts.csproj -- --preflight
```

Ejecutar una etapa:

```powershell
dotnet run --no-build --project .\csharp-sdk-concepts\CsharpSdkConcepts.csproj -- --stage 01 --model gpt-5.4-mini
```

| Etapa | Concepto |
|---|---|
| 01 | Streaming de eventos. |
| 02 | System prompt. |
| 03 | Tool local sin permiso adicional. |
| 04 | Tool local con `approve-once`. |
| 05 | Wikipedia MCP con allowlist. |
| 06 | Skill local cargada desde el repo. |
| 07 | WorkIQ `ask`: chat interactivo con salida agregada y validación determinista. |

La etapa 07 es opcional. Requiere el plugin WorkIQ instalado y su ruta absoluta
en `WORKIQ_PLUGIN_DIR`:

```powershell
$env:WORKIQ_PLUGIN_DIR = 'C:\Users\<your user>\.copilot\installed-plugins\copilot-plugins\workiq'
dotnet run --no-build --project .\csharp-sdk-concepts\CsharpSdkConcepts.csproj -- --stage 07 --model gpt-5.4-mini
```

Abre un chat (`WorkIQ>`) donde se escriben las preguntas; la aplicación muestra
cuatro sugeridas y se sale con `salir`. También acepta `--question "texto"` para
una sola pregunta.

Hay dos barreras: la pregunta se rechaza si pide contenido (asunto, remitente,
cita) y la respuesta se valida contra un contrato JSON agregado. Solo se expone
`workiq-ask`; si la tool falla o el JSON incluye campos no permitidos, la
aplicación bloquea la salida.

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

## Bloque opcional: BYOK con Microsoft Foundry

La consola `byok` usa `ProviderConfig` y Responses API. Los perfiles
`gpt-6.1-sol` y `gpt-6-luna` usan Microsoft Entra; `grok-4.6` usa API key.
Ninguna credencial se imprime o se guarda.

Configurar en un terminal no compartido con dot-sourcing:

```powershell
. .\byok\Set-ByokDemo.ps1 -Model gpt-6.1-sol
# Alternativas: gpt-6-luna o grok-4.6
```

Preparar y ejecutar:

```powershell
dotnet restore .\byok\ByokConsole.csproj --locked-mode
dotnet build .\byok\ByokConsole.csproj --no-restore
dotnet run --no-build --project .\byok\ByokConsole.csproj -- --list-models
dotnet run --no-build --project .\byok\ByokConsole.csproj -- --preflight
dotnet run --no-build --project .\byok\ByokConsole.csproj -- --run
```

Consultar [`byok\README.md`](byok/README.md) antes del directo.

## Otros lenguajes y streaming

- Primeras sesiones mínimas en cinco lenguajes: [`examples`](examples/README.md).
- Guía complementaria: [Streaming de una respuesta — español](docs/02-streaming-es.md).

## Límites

- Los outputs de modelos varían: comprobar eventos, tools y marcadores de
  validación, no prometer texto exacto.
- Un system prompt guía; no autoriza.
- Una allowlist o permiso no equivale a sandbox.
- Las comprobaciones estructurales no prueban exactitud factual.
- No guardar endpoints privados, tokens, claves ni salidas de ensayo en el repo.
