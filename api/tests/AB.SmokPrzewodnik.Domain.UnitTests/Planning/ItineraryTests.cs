using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Planning;

public sealed class ItineraryTests
{
    [Fact]
    public void Constructor_RejectsInvalidTimeRangeAndTimeZone()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => Create(now.AddHours(1), now));
        Assert.Throws<ArgumentException>(() => Create(now, now.AddHours(1), " "));
        Assert.Throws<TimeZoneNotFoundException>(() => Create(now, now.AddHours(1), "Not/A_Time_Zone"));
    }

    [Fact]
    public void AddItem_RejectsDuplicatePositionAndTimesOutsideWindow()
    {
        var itinerary = Create();
        itinerary.AddItem(Item(0), DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Throws<InvalidOperationException>(() =>
            itinerary.AddItem(Item(0), DateTimeOffset.UtcNow.AddMinutes(2)));
        Assert.Throws<ArgumentException>(() =>
            itinerary.AddItem(Item(1, itinerary.StartsAt.AddMinutes(-1)), DateTimeOffset.UtcNow.AddMinutes(2)));
    }

    [Fact]
    public void ItineraryItem_RejectsDepartureBeforeArrival()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => new ItineraryItem(
            Guid.NewGuid(), 0, Guid.NewGuid(), now, now.AddMinutes(-1), null));
    }

    [Fact]
    public void Items_CanBeUpdatedRemovedAndReordered()
    {
        var itinerary = Create();
        var first = Item(0);
        var second = Item(1);
        itinerary.AddItem(first, DateTimeOffset.UtcNow.AddMinutes(1));
        itinerary.AddItem(second, DateTimeOffset.UtcNow.AddMinutes(2));

        itinerary.UpdateItem(
            new ItineraryItem(first.Id, first.Position, first.EntityId, first.PlannedArrival, first.PlannedDeparture, "Meet by the entrance"),
            DateTimeOffset.UtcNow.AddMinutes(3));
        itinerary.ReorderItems([second.Id, first.Id], DateTimeOffset.UtcNow.AddMinutes(4));
        itinerary.RemoveItem(first.Id, DateTimeOffset.UtcNow.AddMinutes(5));

        var remaining = Assert.Single(itinerary.Items);
        Assert.Equal(second.Id, remaining.Id);
        Assert.Equal(0, remaining.Position);
    }

    [Fact]
    public void RouteLegReferences_AreReplacedAndDeduplicated()
    {
        var itinerary = Create();
        var reference = Guid.NewGuid();

        itinerary.ReplaceRouteLegReferences([reference, reference], DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(reference, Assert.Single(itinerary.RouteLegReferences));
        Assert.True(Assert.IsAssignableFrom<ICollection<Guid>>(itinerary.RouteLegReferences).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ICollection<ItineraryItem>>(itinerary.Items).IsReadOnly);
    }

    private static Itinerary Create() => Create(
        DateTimeOffset.UtcNow.AddHours(1),
        DateTimeOffset.UtcNow.AddHours(5));

    private static Itinerary Create(DateTimeOffset startsAt, DateTimeOffset endsAt, string timeZone = "Europe/Warsaw") =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new LocalizedContent(new Dictionary<string, string> { ["pl-PL"] = "Plan dnia" }),
            startsAt,
            endsAt,
            timeZone,
            [],
            [],
            DateTimeOffset.UtcNow);

    private static ItineraryItem Item(int position, DateTimeOffset? arrival = null) =>
        new(Guid.NewGuid(), position, Guid.NewGuid(), arrival, arrival?.AddMinutes(15), null);
}
