using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Planning;

public sealed class ProjectionTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Suggestion_RejectsScoreOutsideUnitRange(double score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SuggestionProjection(
            Guid.NewGuid(), Guid.NewGuid(), [new Code("nearby")], (decimal)score,
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1)));
    }

    [Fact]
    public void Suggestion_RejectsEmptyReasonsAndInvalidValidityRange()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => new SuggestionProjection(
            Guid.NewGuid(), Guid.NewGuid(), [], 0.5m, now, now.AddHours(1)));
        Assert.Throws<ArgumentException>(() => new SuggestionProjection(
            Guid.NewGuid(), Guid.NewGuid(), [new Code("nearby")], 0.5m, now, now.AddMinutes(-1)));
    }

    [Fact]
    public void FeedItem_RequiresLinkedEntitiesAndAudienceTags()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => Feed([], [new Code("wheelchair")], now.AddHours(1)));
        Assert.Throws<ArgumentException>(() => Feed([Guid.NewGuid()], [], now.AddHours(1)));
        Assert.Throws<ArgumentException>(() => Feed([Guid.NewGuid()], [new Code("wheelchair")], now.AddHours(-1)));
    }

    [Fact]
    public void ProjectionCollections_AreImmutableSnapshots()
    {
        var linkedIds = new List<Guid> { Guid.NewGuid() };
        var tags = new List<Code> { new("wheelchair") };
        var feed = Feed(linkedIds, tags, DateTimeOffset.UtcNow.AddHours(1));
        linkedIds.Add(Guid.NewGuid());
        tags.Add(new Code("family"));

        Assert.Single(feed.LinkedEntityIds);
        Assert.Single(feed.AudienceTags);
        Assert.True(Assert.IsAssignableFrom<ICollection<Guid>>(feed.LinkedEntityIds).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ISet<Code>>(feed.AudienceTags).IsReadOnly);
    }

    private static FeedItem Feed(
        IEnumerable<Guid> linkedIds,
        IEnumerable<Code> tags,
        DateTimeOffset validUntil)
    {
        var publishedAt = DateTimeOffset.UtcNow;
        return new FeedItem(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Code("event_update"),
            Guid.NewGuid(),
            linkedIds,
            new LocalizedContent(new Dictionary<string, string> { ["pl-PL"] = "Zmiana miejsca" }),
            publishedAt,
            validUntil,
            tags,
            new Code("municipal_feed"));
    }
}
