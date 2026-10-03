using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed class Itinerary : AggregateRoot<Guid>
{
    private readonly List<ItineraryItem> _items;
    private readonly ReadOnlyCollection<ItineraryItem> _itemsView;
    private readonly List<Guid> _routeLegReferences;
    private readonly ReadOnlyCollection<Guid> _routeLegReferencesView;

    private Itinerary()
    {
        LocalizedTitle = null!;
        TimeZone = string.Empty;
        _items = [];
        _itemsView = _items.AsReadOnly();
        _routeLegReferences = [];
        _routeLegReferencesView = _routeLegReferences.AsReadOnly();
    }

    public Itinerary(
        Guid id,
        Guid accountId,
        LocalizedContent localizedTitle,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string timeZone,
        IEnumerable<ItineraryItem> items,
        IEnumerable<Guid> routeLegReferences,
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt)
        : base(RequireId(id, nameof(id)), createdAt, updatedAt)
    {
        AccountId = RequireId(accountId, nameof(accountId));
        LocalizedTitle = localizedTitle ?? throw new ArgumentNullException(nameof(localizedTitle));
        TimeZone = ValidateTimeZone(timeZone);
        ValidateRange(startsAt, endsAt);
        StartsAt = startsAt;
        EndsAt = endsAt;

        ArgumentNullException.ThrowIfNull(items);
        _items = items.OrderBy(item => item.Position).ToList();
        EnsureValidItems(_items);
        _itemsView = _items.AsReadOnly();

        ArgumentNullException.ThrowIfNull(routeLegReferences);
        _routeLegReferences = routeLegReferences.Distinct().Select(id => RequireId(id, nameof(routeLegReferences))).ToList();
        _routeLegReferencesView = _routeLegReferences.AsReadOnly();
    }

    public Guid AccountId { get; private set; }

    public LocalizedContent LocalizedTitle { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public string TimeZone { get; private set; }

    public IReadOnlyList<ItineraryItem> Items => _itemsView;

    public IReadOnlyList<Guid> RouteLegReferences => _routeLegReferencesView;

    public static Itinerary Create(
        Guid accountId,
        LocalizedContent localizedTitle,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        string timeZone,
        DateTimeOffset createdAt) =>
        new(Guid.NewGuid(), accountId, localizedTitle, startsAt, endsAt, timeZone, [], [], createdAt, null);

    public void Rename(LocalizedContent localizedTitle, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(localizedTitle);
        Touch(updatedAt);
        LocalizedTitle = localizedTitle;
    }

    public void Reschedule(DateTimeOffset startsAt, DateTimeOffset endsAt, string timeZone, DateTimeOffset updatedAt)
    {
        ValidateRange(startsAt, endsAt);
        var validatedTimeZone = ValidateTimeZone(timeZone);

        if (_items.Any(item => !IsWithinRange(item, startsAt, endsAt)))
        {
            throw new InvalidOperationException("Existing itinerary items would fall outside the new time range.");
        }

        Touch(updatedAt);
        StartsAt = startsAt;
        EndsAt = endsAt;
        TimeZone = validatedTimeZone;
    }

    public void AddItem(ItineraryItem item, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureItemFits(item, null);
        Touch(updatedAt);
        _items.Add(item);
        _items.Sort((left, right) => left.Position.CompareTo(right.Position));
    }

    public void UpdateItem(ItineraryItem item, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = _items.FindIndex(existing => existing.Id == item.Id);
        if (index < 0)
        {
            throw new KeyNotFoundException("The itinerary item does not exist.");
        }

        EnsureItemFits(item, item.Id);
        Touch(updatedAt);
        _items[index] = item;
        _items.Sort((left, right) => left.Position.CompareTo(right.Position));
    }

    public void RemoveItem(Guid itemId, DateTimeOffset updatedAt)
    {
        var index = _items.FindIndex(item => item.Id == itemId);
        if (index < 0)
        {
            throw new KeyNotFoundException("The itinerary item does not exist.");
        }

        Touch(updatedAt);
        _items.RemoveAt(index);
        NormalizePositions();
    }

    public void ReorderItems(IEnumerable<Guid> itemIds, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        var orderedIds = itemIds.ToList();
        if (orderedIds.Count != _items.Count || orderedIds.Distinct().Count() != orderedIds.Count ||
            orderedIds.Any(id => _items.All(item => item.Id != id)))
        {
            throw new ArgumentException("The ordering must contain every item exactly once.", nameof(itemIds));
        }

        Touch(updatedAt);
        var byId = _items.ToDictionary(item => item.Id);
        _items.Clear();
        _items.AddRange(orderedIds.Select((id, position) => byId[id].AtPosition(position)));
    }

    public void ReplaceRouteLegReferences(IEnumerable<Guid> routeLegReferences, DateTimeOffset updatedAt)
    {
        ArgumentNullException.ThrowIfNull(routeLegReferences);
        var references = routeLegReferences.Distinct().Select(id => RequireId(id, nameof(routeLegReferences))).ToList();
        Touch(updatedAt);
        _routeLegReferences.Clear();
        _routeLegReferences.AddRange(references);
    }

    private void EnsureValidItems(IReadOnlyCollection<ItineraryItem> items)
    {
        if (items.Select(item => item.Id).Distinct().Count() != items.Count ||
            items.Select(item => item.Position).Distinct().Count() != items.Count)
        {
            throw new ArgumentException("Itinerary item identifiers and positions must be unique.", nameof(items));
        }

        foreach (var item in items)
        {
            EnsureItemFits(item, item.Id);
        }
    }

    private void EnsureItemFits(ItineraryItem item, Guid? replacedItemId)
    {
        if (_items.Any(existing => existing.Id == item.Id && existing.Id != replacedItemId))
        {
            throw new InvalidOperationException("An item with this identifier already exists.");
        }

        if (_items.Any(existing => existing.Position == item.Position && existing.Id != replacedItemId))
        {
            throw new InvalidOperationException("An item with this position already exists.");
        }

        if (!IsWithinRange(item, StartsAt, EndsAt))
        {
            throw new ArgumentException("Item times must be within the itinerary time range.", nameof(item));
        }
    }

    private static bool IsWithinRange(ItineraryItem item, DateTimeOffset startsAt, DateTimeOffset endsAt) =>
        (!item.PlannedArrival.HasValue || item.PlannedArrival >= startsAt && item.PlannedArrival <= endsAt) &&
        (!item.PlannedDeparture.HasValue || item.PlannedDeparture >= startsAt && item.PlannedDeparture <= endsAt);

    private void NormalizePositions()
    {
        for (var position = 0; position < _items.Count; position++)
        {
            _items[position] = _items[position].AtPosition(position);
        }
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        if (updatedAt < UpdatedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(updatedAt), "The update timestamp cannot move backwards.");
        }

        MarkUpdated(updatedAt);
    }

    private static void ValidateRange(DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        if (endsAt < startsAt)
        {
            throw new ArgumentException("The itinerary end cannot precede its start.", nameof(endsAt));
        }
    }

    private static string ValidateTimeZone(string timeZone)
    {
        var value = Guard.NotBlank(timeZone, nameof(timeZone));
        _ = TimeZoneInfo.FindSystemTimeZoneById(value);
        return value;
    }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }
}
