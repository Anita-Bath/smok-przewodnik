using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class EntityTranslation : Entity<Guid>
{
    public EntityTranslation(Guid id, Guid entityId, string locale, string name, string description)
        : base(id)
    {
        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity ID cannot be empty.", nameof(entityId));
        }

        EntityId = entityId;
        Locale = Guard.NotBlank(locale, nameof(locale));
        Name = Guard.NotBlank(name, nameof(name));
        Description = Guard.NotBlank(description, nameof(description));
    }

    public Guid EntityId { get; }
    public SpatialEntity Entity { get; private set; } = null!;
    public string Locale { get; }
    public string Name { get; }
    public string Description { get; }
}
