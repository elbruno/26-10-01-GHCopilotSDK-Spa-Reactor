# Accessibility Reviewer — GitHub Copilot SDK

Adaptación .NET del [workshop oficial](https://github.com/github/copilot-sdk-workshop/tree/716bdaaf629817606873b4e22d551e900985c92c).
Ver `THIRD-PARTY-NOTICES.txt`. Proyecto único, etapas numeradas; no escribir
todo el código en vivo.

## Preparación

Requisitos: .NET 10 SDK, Node.js 22 o superior, GitHub Copilot CLI
autenticado y Microsoft Edge o Google Chrome. Desde la raíz de este repositorio:

```powershell
.\Initialize-DemoRepo.ps1
```

El script valida las versiones, restaura dependencias, compila, instala
Playwright MCP localmente y ejecuta las comprobaciones sin llamar al modelo.
No se necesita un entorno virtual de Python. Si falta autenticación, ejecutar
`copilot login` fuera de cámara y volver a ejecutar el script.

Comprobaciones equivalentes, si se desea ejecutarlas por separado:

```powershell
dotnet restore .\AccessibilityDemo.csproj --locked-mode
dotnet build .\AccessibilityDemo.csproj --no-restore
npm ci --no-audit --no-fund
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --self-test
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --preflight
```

En un terminal aparte, mantener la página mientras se ejecutan 05, 06 o 99:

```powershell
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --serve
```

Abrir `http://127.0.0.1:4173/` en el navegador que se comparte. La inspección MCP
usa **otro Edge, headless y aislado**, no el perfil personal.
El servidor solo sirve esta página, sin archivos arbitrarios, redirecciones,
scripts ni recursos externos. Detener con Ctrl+C al terminar.

## Etapas

```powershell
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --stage 01 --model gpt-5.4-mini
```

Cambiar solo `01` por la etapa elegida:

| Etapa | Archivo a mostrar en `Stages` | Resultado observable |
|---|---|---|
| 01 | `01-FirstSession.cs` | Respuesta completa, sin tools. |
| 02 | `02-Streaming.cs` | Texto incremental; contador de deltas mayor que cero. |
| 03 | `03-SystemPrompt.cs` | Rol persistente, evidencia frente a hipótesis. |
| 04 | `04-LocalTool.cs` | Permiso `custom-tool`, handler 4.1.2 y éxito real. |
| 05 | `05-Mcp.cs` | Navegación MCP autorizada y lectura del snapshot de esta ejecución. |
| 06 | `06-Permissions.cs` | Primero permitido; luego `/blocked` denegado, tool sin éxito. |
| 99 | `99-Finished.cs` | Respaldo completo: persona, streaming, MCP, catálogo y denegación. |

`Program.cs` muestra cliente → autenticación → configuración → sesión →
eventos → turno → comprobaciones → dispose. `Runtime\DemoPolicy.cs` muestra
la autorización efectiva; el system prompt no es la barrera de seguridad.
Las tools locales no omiten permisos. La única tool MCP expuesta es
`browser_navigate`; no hay shell, escritura, clicks, evaluación de JavaScript
ni navegación arbitraria. Todo lo no reconocido se deniega.

Los criterios WCAG del catálogo son orientación limitada, no una auditoría.
El snapshot no prueba contraste ni teclado. La allowlist de origen de Playwright
es defensa adicional, **no sandbox de red**; no garantiza bloquear redirecciones.
La URL exacta se valida en el host y la página local no redirige ni carga recursos
externos. No reutilizar esta política para sitios arbitrarios.

Cada turno tiene límite de 90 s y cada proceso, 180 s; Ctrl+C cancela.
Un error produce salida `[ERROR]` y código distinto de cero, nunca `[verified]`.
Los snapshots de cada proceso están en un directorio nuevo bajo
`bin\Debug\net10.0\.runs`; el lector limita tamaño, rechaza enlaces y no reutiliza
un snapshot consumido. No son evidencia de una ejecución posterior.

## Respaldo y ensayo

```powershell
# Estado terminado ya compilado; no editarlo en el directo.
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --stage 99 --model gpt-5.4-mini

# Con el servidor local en el otro terminal: ejecuta y guarda las siete etapas
# en .runs\rehearsal.
.\Rehearse.ps1

# Volver a comprobar solo MCP/permisos y respaldo.
.\Rehearse.ps1 -Stages 05,06,99
```

`--no-build` utiliza el último binario compilado aunque una edición del código
falle al compilar. No recompilar una edición no ensayada antes de usar el respaldo.
99 es un **estado terminado del código**, no elimina la dependencia de
autenticación, cuota, modelo y MCP. En este repositorio público no se incluyen
salidas de ensayo ni credenciales.

Las salidas varían: comprobar los marcadores de eventos/handlers y `[verified]`,
no prometer un texto exacto. No hay hooks/custom agents/BYOK implementados:
tratarlos solo conceptualmente.
