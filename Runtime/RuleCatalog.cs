// Catalogo local de reglas WCAG para Accessibility.
// Enseña CopilotTool.DefineTool: una funcion .NET se publica como tool legible
// por el modelo, pero sigue ejecutandose dentro del host de la aplicacion.
// Se usa en la etapa 04 y en el estado 99.
using System.ComponentModel;
using GitHub.Copilot;
using Microsoft.Extensions.AI;

namespace AccessibilityDemo.Runtime;

public static class RuleCatalog
{
    public sealed record Rule(string Criterion, string Title, string Recommendation);

    // El catalogo pequeño evita depender del modelo para recordar reglas durante el directo.
    public static Rule Lookup(string criterion) => criterion switch
    {
        "1.1.1" => new(criterion, "Non-text Content", "Proporciona texto alternativo para imagenes informativas."),
        "1.3.1" => new(criterion, "Info and Relationships", "Usa estructura semantica y relaciones programaticas."),
        "1.4.3" => new(criterion, "Contrast (Minimum)", "Mide contraste: 4.5:1 texto normal, 3:1 texto grande."),
        "2.4.7" => new(criterion, "Focus Visible", "Comprueba un indicador visible de foco con teclado."),
        "3.3.2" => new(criterion, "Labels or Instructions", "Proporciona etiquetas visibles e instrucciones."),
        "4.1.2" => new(criterion, "Name, Role, Value", "Asocia un label visible con el input usando for e id."),
        _ => throw new ArgumentException("Criterio no disponible en el catalogo limitado de la demo.")
    };

    // DefineTool describe el contrato que el SDK ofrece al modelo en la sesion.
    public static AIFunction Create(Action onLookup) => CopilotTool.DefineTool(
        ([Description("WCAG criterion: 1.1.1, 1.3.1, 1.4.3, 2.4.7, 3.3.2 or 4.1.2.")] string criterion) =>
        {
            var rule = Lookup(criterion);
            onLookup();
            Console.WriteLine($"\n[handler:rule] {rule.Criterion}");
            return Task.FromResult(rule);
        },
        // SkipPermission=false fuerza que OnPermissionRequest pueda autorizar la lectura.
        toolOptions: new CopilotToolOptions { SkipPermission = false },
        factoryOptions: new AIFunctionFactoryOptions
        {
            Name = "accessibility_rule_lookup",
            Description = "Read-only lookup in the application's small WCAG guidance catalog."
        });
}
