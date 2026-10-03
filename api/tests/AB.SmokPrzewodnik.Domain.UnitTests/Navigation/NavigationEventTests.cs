using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Navigation;

public sealed class NavigationEventTests
{
    [Fact]
    public void Event_RequiresPositiveSequenceAndNonnegativeDistance()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(sequence: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(remainingDistance: -1));
    }

    [Fact]
    public void Event_PreservesStructuredCuesAndFeedbackPatterns()
    {
        var maneuver = new Maneuver(
            0, new Code("turn_left"), new GeoCoordinate(50, 19), 15, TimeSpan.FromSeconds(8));
        var navigationEvent = Create(maneuver: maneuver);

        Assert.Equal(maneuver, navigationEvent.Maneuver);
        Assert.Equal(new Code("roadworks"), navigationEvent.HazardCode);
        Assert.Equal(new Code("clock_tower"), navigationEvent.LandmarkCode);
        Assert.Equal("15", navigationEvent.LocalizedParameters["distance"]);
        Assert.Contains(new Code("double_pulse"), navigationEvent.SupportedFeedbackPatterns);
        Assert.True(Assert.IsAssignableFrom<ISet<Code>>(navigationEvent.SupportedFeedbackPatterns).IsReadOnly);
    }

    private static NavigationEvent Create(
        long sequence = 1,
        decimal? remainingDistance = 15,
        Maneuver? maneuver = null) =>
        new(
            Guid.NewGuid(),
            sequence,
            NavigationEventType.Hazard,
            NavigationUrgency.Immediate,
            maneuver,
            new Code("roadworks"),
            new Code("clock_tower"),
            remainingDistance,
            new Dictionary<string, string> { ["distance"] = "15" },
            [new Code("double_pulse")]);
}
