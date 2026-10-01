// Etapa Museum 05 — Validacion determinista.
// Enseña que el SDK genera texto, pero el host comprueba estructura medible y
// falla si el contrato de la aplicacion no se cumple.
namespace MuseumExhibitStudio.Stages;

public static class ProveStructure
{
    public static MuseumStage Create() => new(
        "05",
        "Validacion determinista",
        MuseumPrompts.Exhibit,
        Streaming: true,
        Persona: true,
        Facts: true,
        Validate: true);
}
