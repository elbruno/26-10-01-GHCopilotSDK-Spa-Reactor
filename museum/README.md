# Museum Exhibit Studio — demo principal

Adaptación progresiva en .NET del recorrido Museum Exhibit Studio del
[workshop oficial](https://github.com/github/copilot-sdk-workshop/tree/716bdaaf629817606873b4e22d551e900985c92c).

```powershell
dotnet restore .\MuseumDemo.csproj --locked-mode
npm ci --no-audit --no-fund
dotnet build .\MuseumDemo.csproj --no-restore
dotnet run --no-build --project .\MuseumDemo.csproj -- --self-test
dotnet run --no-build --project .\MuseumDemo.csproj -- --preflight
```

Ejecutar una etapa:

```powershell
dotnet run --no-build --project .\MuseumDemo.csproj -- --stage 01 --model gpt-5.4-mini
```

| Etapa | Concepto |
|---|---|
| 01 | Cliente, runtime y primera sesión. |
| 02 | Streaming mediante eventos. |
| 03 | System prompt y voz del curador. |
| 04 | Tool local con hechos aprobados. |
| 05 | Validación estructural determinista. |
| 06 | Sesión MCP separada para investigación en Wikipedia. |
| 99 | Investigación opcional, generación grounded y validación. |

`99` también se puede ejecutar con `--review`. La investigación nunca modifica
los hechos aprobados usados para generar la exhibición.
