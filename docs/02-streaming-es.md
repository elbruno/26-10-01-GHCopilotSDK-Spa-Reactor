# Streaming de una respuesta — guía en español

Guía complementaria en español para
[Stream a response · .NET](https://github.github.com/copilot-sdk-workshop/workshop/step.html?step=02-streaming&lang=dotnet).
No sustituye la lección oficial.

## Qué cambia

Una respuesta completada espera hasta que el turno termina. Con streaming, la
aplicación escucha eventos y puede mostrar cada fragmento a medida que llega.
Esto permite actualizar una consola, una interfaz web o cualquier UI propia.

## 1. Activar streaming en la sesión

En [`Stages/02-Streaming.cs`](../Stages/02-Streaming.cs), `Streaming: true`
indica que la sesión debe emitir deltas:

```csharp
public static DemoStage Create() => new(
    "02",
    "Streaming",
    "Explica en español cuatro comprobaciones de accesibilidad de un formulario.",
    Streaming: true);
```

## 2. Escuchar los eventos

En [`Program.cs`](../Program.cs), la aplicación se suscribe a los eventos de la
sesión. Cada `AssistantMessageDeltaEvent` contiene un fragmento:

```csharp
case AssistantMessageDeltaEvent delta when stage.Streaming:
    if (!string.IsNullOrEmpty(delta.Data.DeltaContent))
    {
        deltas++;
        Console.Write(delta.Data.DeltaContent);
    }
    break;
```

El contador no es parte del SDK: esta aplicación lo usa para comprobar que
realmente recibió streaming y no solo una respuesta final.

## 3. Ejecutar el checkpoint

```powershell
dotnet run --no-build --project .\AccessibilityDemo.csproj -- --stage 02 --model gpt-5.4-mini
```

La salida debe aparecer progresivamente y terminar con un marcador similar a:

```text
[verified] stage=02 deltas=...
```

## Por qué importa en una aplicación

En Copilot Chat la interfaz de streaming ya está construida. Con Copilot SDK,
tu aplicación recibe los eventos y decide cómo representarlos: texto incremental,
indicadores de progreso, actividad de tools, telemetría o estados de una tarea.

La demo principal del stream usa `--review`, que combina streaming con tools,
MCP, permisos y validación. La etapa 02 queda como checkpoint para aislar y
entender este concepto.
