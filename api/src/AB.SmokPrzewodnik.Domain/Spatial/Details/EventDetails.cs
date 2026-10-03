using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record EventDetails : SpatialEntityDetails
{
    private readonly List<EventCategory> _categories = [];

    private EventDetails() : base(EntityKind.Event)
    {
    }

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
        var categories = categoryCodes.ToFrozenSet();
        if (categories.Count == 0)
        {
            throw new ArgumentException("At least one event category is required.", nameof(categoryCodes));
        }

        _categories.AddRange(categories
            .Select(category => new EventCategory(category))
            .OrderBy(category => category.Code.Value, StringComparer.Ordinal));

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

    public IReadOnlySet<Code> CategoryCodes => _categories.Select(category => category.Code).ToFrozenSet();
    internal IReadOnlyCollection<EventCategory> Categories => _categories;
    public Guid? OrganizerEntityId { get; private set; }
    public SpatialEntity? Organizer { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public DateTimeOffset EndsAt { get; private set; }
    public Uri? BookingUri { get; private set; }
    public uint? Capacity { get; private set; }
}
