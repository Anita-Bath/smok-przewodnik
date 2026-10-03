namespace AB.SmokPrzewodnik.Domain.Spatial;

public abstract record AccessibilityFactTarget
{
    private AccessibilityFactTarget()
    {
    }

    public sealed record SpatialEntity : AccessibilityFactTarget
    {
        public SpatialEntity(Guid entityId)
        {
            if (entityId == Guid.Empty)
            {
                throw new ArgumentException("Entity ID cannot be empty.", nameof(entityId));
            }

            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }

    public sealed record GraphElement(GraphReference Reference) : AccessibilityFactTarget;
}
