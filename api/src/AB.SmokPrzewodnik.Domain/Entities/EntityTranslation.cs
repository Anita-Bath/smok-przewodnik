using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Entities;

public class EntityTranslation(Guid id) : Entity<Guid>(id)
{
    public Guid EntityId { get; set; }
    public string Locale { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public EntityTranslation(Guid entityId, string locale, string name, string description)
    {
        EntityId = entityId;
        Locale = locale;
        Name = name;
        Description = description;
    }
}
