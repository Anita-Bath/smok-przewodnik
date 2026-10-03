using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record EventDetails : SpatialEntityDetails
{
    public EventDetails(
        IEnumerable<Code> categoryCodes,
        Guid? organizerEntityId,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        Uri? bookingUri,
        uint? capacity)
        : base(EntityKind.Event)
    {
        ArgumentNullException.ThrowIfNull(categoryCodes);
        CategoryCodes = categoryCodes.ToFrozenSet();
        if (CategoryCodes.Count == 0)
        {
            throw new ArgumentException("At least one event category is required.", nameof(categoryCodes));
        }

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

    public IReadOnlySet<Code> CategoryCodes { get; }
    public Guid? OrganizerEntityId { get; }
    public SpatialEntity? Organizer { get; private set; }
    public DateTimeOffset StartsAt { get; }
    public DateTimeOffset EndsAt { get; }
    public Uri? BookingUri { get; }
    public uint? Capacity { get; }
}
