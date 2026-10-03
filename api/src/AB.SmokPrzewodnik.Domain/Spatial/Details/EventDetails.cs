using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record EventDetails : SpatialEntityDetails
{
    public EventDetails(
        Guid? organizerEntityId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Uri? bookingUri,
        uint? capacity)
        : base(EntityKind.Event)
    {
        if (endsAt < startsAt)
        {
            throw new ArgumentException("Event end cannot precede its start.", nameof(endsAt));
        }

        OrganizerEntityId = organizerEntityId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        BookingUri = bookingUri;
        Capacity = capacity;
    }

    public Guid? OrganizerEntityId { get; }
    public DateTimeOffset StartsAt { get; }
    public DateTimeOffset EndsAt { get; }
    public Uri? BookingUri { get; }
    public uint? Capacity { get; }
}
