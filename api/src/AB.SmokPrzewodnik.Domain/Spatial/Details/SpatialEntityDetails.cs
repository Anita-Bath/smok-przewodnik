using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public abstract record SpatialEntityDetails
{
    protected SpatialEntityDetails(EntityKind kind)
    {
        Kind = kind;
    }

    protected SpatialEntityDetails()
    {
    }

    public EntityKind Kind { get; private set; }
    internal Guid SpatialEntityId { get; private set; }
}
