# BYOK con Microsoft Foundry

Esta consola demuestra que GitHub Copilot SDK mantiene el runtime y cambia el
proveedor de inferencia mediante `ProviderConfig`.

## Qué muestra

1. `CopilotClient.ListModelsAsync()` consulta el catálogo del runtime Copilot.
2. `ProviderConfig` selecciona explícitamente el modelo que recibirá Foundry.
3. `/openai/v1/models` comprueba el catálogo del proveedor sin gastar una
   inferencia.
4. La misma aplicación puede usar Microsoft Entra o API key.

Los deployments BYOK no se descubren automáticamente con `ListModelsAsync()`.
Ese método devuelve modelos y metadatos que el runtime sabe orquestar.

## Perfiles preparados

| Modelo | Autenticación |
|---|---|
| `gpt-6.1-sol` | Microsoft Entra mediante `AzureCliCredential` |
| `gpt-6-luna` | Microsoft Entra mediante `AzureCliCredential` |
| `grok-4.6` | API key con header `api-key` |

## Configuración segura

Ejecutar fuera de cámara con **dot-sourcing**:

```powershell
. .\Set-ByokDemo.ps1 -Model gpt-6.1-sol
```

Alternativas:

```powershell
. .\Set-ByokDemo.ps1 -Model gpt-6-luna
. .\Set-ByokDemo.ps1 -Model grok-4.6
```

El script pide la URL si `FOUNDRY_RESOURCE_URL` no existe. Para Grok pide la
key con entrada oculta. Las variables viven solo en el proceso actual:

- `FOUNDRY_RESOURCE_URL`;
- `FOUNDRY_MODEL`;
- `FOUNDRY_AUTH_MODE`;
- `FOUNDRY_API_KEY`, solo para el perfil `api-key`.

No guardar estas variables, endpoints o credenciales en el repositorio.

## Preparar y ejecutar

```powershell
dotnet restore .\ByokConsole.csproj --locked-mode
dotnet build .\ByokConsole.csproj --no-restore

# Catálogo que conoce el runtime Copilot:
dotnet run --no-build --project .\ByokConsole.csproj -- --list-models

# Runtime + catálogo Foundry, sin prompt:
dotnet run --no-build --project .\ByokConsole.csproj -- --preflight

# Inferencia real en Foundry:
dotnet run --no-build --project .\ByokConsole.csproj -- --run
```

La salida no imprime endpoint, token ni API key. El marcador final indica solo
proveedor, modo de autenticación y modelo.
