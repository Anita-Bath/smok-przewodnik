using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Navigation;

public sealed class NavigationSessionTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public void Constructor_CopiesRouteStateAndStartsSequenceAndVersion()
    {
        var route = Alternative();
        var request = Request();
        var session = new NavigationSession(Guid.NewGuid(), "hash", Guid.NewGuid(), route, request, Now, Now.AddHours(1));

        Assert.Same(route, session.ActiveRoute);
        Assert.Same(request, session.EffectiveRouteRequest);
        Assert.Equal(1, session.NextEventSequence);
        Assert.Equal(0, session.Version);
        Assert.Null(session.LatestProgress);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_RejectsBlankHash(string hash) =>
        Assert.Throws<ArgumentException>(() => new NavigationSession(
            Guid.NewGuid(), hash, null, Alternative(), Request(), Now, Now.AddHours(1)));

    [Fact]
    public void Constructor_RejectsExpiredSession() =>
        Assert.Throws<ArgumentException>(() => new NavigationSession(
            Guid.NewGuid(), "hash", null, Alternative(), Request(), Now, Now));

    private static RoutePlanRequest Request() => new(
        new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
        new RouteEndpoint.Coordinate(new GeoCoordinate(51m, 20m)),
        [TravelMode.Walk], new Dictionary<Code, ConstraintLevel>(), [new Code("fastest")], "pl-PL",
        new Dictionary<Code, string>());

    private static RouteAlternative Alternative() => new(
        Guid.NewGuid(), new Code("fastest"),
        new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(51m, 20m)]),
        [new RouteLeg(Guid.NewGuid(), 0,
            new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(51m, 20m)]),
            TimeSpan.FromMinutes(1), 10m, [])],
        TimeSpan.FromMinutes(1), 10m, [], new AccessibilitySummary([], [], []), [],
        new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, Now), new Dictionary<Code, string>(),
        Now, Now.AddMinutes(15));
}
