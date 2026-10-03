using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed class EventCategory
{
    private EventCategory()
    {
        Code = default;
    }

    internal EventCategory(Code code)
    {
        Code = new Code(code.Value.ToLowerInvariant());
    }

    public Code Code { get; private set; }
    internal Guid SpatialEntityId { get; private set; }
}
