namespace AB.SmokPrzewodnik.Domain.Entities;

public abstract record AccessibilityFactTarget
{
    public sealed record SpatialEntity(Guid EntityId)
        : AccessibilityFactTarget;

    public sealed record GraphElement(GraphReference Reference)
        : AccessibilityFactTarget;
}
