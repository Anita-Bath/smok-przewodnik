using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Spatial;

public sealed class EventDetailsTests
{
    [Fact]
    public void Constructor_RequiresAtLeastOneCategory()
    {
        var startsAt = DateTimeOffset.Parse("2026-10-10T18:00:00Z");

        Assert.Throws<ArgumentException>(() => new EventDetails(
            [],
            null,
            startsAt,
            startsAt.AddHours(2),
            null,
            null));
    }

    [Fact]
    public void Constructor_CopiesAndDeduplicatesCategories()
    {
        var startsAt = DateTimeOffset.Parse("2026-10-10T18:00:00Z");
        var categories = new List<Code>
        {
            new("concert"),
            new("concert"),
            new("accessible_family")
        };

        var details = new EventDetails(
            categories,
            null,
            startsAt,
            startsAt.AddHours(2),
            null,
            null);
        categories.Add(new Code("workshop"));

        Assert.Equal(2, details.CategoryCodes.Count);
        Assert.Contains(new Code("concert"), details.CategoryCodes);
        Assert.Contains(new Code("accessible_family"), details.CategoryCodes);
        Assert.DoesNotContain(new Code("workshop"), details.CategoryCodes);
    }
}
