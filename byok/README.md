# BYOK con Microsoft Foundry y Microsoft Entra

Esta consola demuestra que GitHub Copilot SDK puede usar el runtime de Copilot
con un proveedor de modelos propio. La autenticación del modelo usa
`AzureCliCredential`; no usa una API key ni imprime tokens.

Requisitos:

1. GitHub Copilot CLI instalado. BYOK evita la autenticación GitHub Copilot para
   el modelo, pero todavía usa el runtime local.
2. Azure CLI autenticado fuera de cámara con acceso al recurso Foundry.
3. Un deployment compatible con Responses API.

Configurar solo en el terminal no compartido:

```powershell
$env:FOUNDRY_RESOURCE_URL = 'https://<resource>.openai.azure.com'
$env:FOUNDRY_MODEL = '<deployment-name>'
```

Preparar y comprobar Microsoft Entra sin llamar al modelo:

```powershell
dotnet restore .\ByokConsole.csproj --locked-mode
dotnet build .\ByokConsole.csproj --no-restore
dotnet run --no-build --project .\ByokConsole.csproj -- --preflight
```

Ejecutar:

```powershell
dotnet run --no-build --project .\ByokConsole.csproj -- --run
```

El código configura `ProviderConfig` con:

- `Type = "openai"`;
- endpoint Microsoft Foundry `/openai/v1/`;
- `WireApi = "responses"`;
- `BearerTokenProvider` respaldado por Microsoft Entra y Azure CLI.

Las variables contienen configuración, no deben guardarse en el repositorio.
El token se obtiene bajo demanda y nunca se escribe en consola.
