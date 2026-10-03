using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Routing;

public sealed class RouteModelTests
{
    [Fact]
    public void EndpointVariants_PreserveCoordinateOrEntity()
    {
        var coordinate = new GeoCoordinate(50.0617m, 19.9373m);
        var entityId = Guid.NewGuid();

        Assert.Equal(coordinate, Assert.IsType<RouteEndpoint.Coordinate>(new RouteEndpoint.Coordinate(coordinate)).Value);
        Assert.Equal(entityId, Assert.IsType<RouteEndpoint.Entity>(new RouteEndpoint.Entity(entityId)).EntityId);
        Assert.Throws<ArgumentException>(() => new RouteEndpoint.Entity(Guid.Empty));
    }

    [Fact]
    public void Request_RequiresTravelModeAndUniqueProfiles()
    {
        Assert.Throws<ArgumentException>(() => Request([]));
        Assert.Throws<ArgumentException>(() => Request(
            [TravelMode.Walk], [new Code("fastest"), new Code("fastest")]));
    }

    [Fact]
    public void Request_CollectionsAreImmutableSnapshots()
    {
        var constraints = new Dictionary<Code, ConstraintLevel> { [new Code("stairs")] = ConstraintLevel.MustAvoid };
        var versions = new Dictionary<Code, string> { [new Code("krakow")] = "42" };
        var request = new RoutePlanRequest(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50, 19)),
            new RouteEndpoint.Entity(Guid.NewGuid()),
            [TravelMode.Walk],
            constraints,
            [new Code("easiest")],
            "pl-PL",
            versions);
        constraints.Clear();
        versions.Clear();

        Assert.Single(request.Constraints);
        Assert.Single(request.ClientDataVersions);
        Assert.True(Assert.IsAssignableFrom<ISet<TravelMode>>(request.TravelModes).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ICollection<KeyValuePair<Code, ConstraintLevel>>>(request.Constraints).IsReadOnly);
    }

    [Fact]
    public void NumericRouteValues_CannotBeNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CostComponent(new Code("time"), -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Maneuver(
            0, new Code("turn_left"), new GeoCoordinate(50, 19), -1, TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RouteLeg(
            Guid.NewGuid(), 0, Geometry(), TimeSpan.FromMinutes(-1), 10, []));
    }

    [Fact]
    public void RouteLeg_RequiresUniqueOrderedManeuvers()
    {
        var maneuver = Maneuver(0);

        Assert.Throws<ArgumentException>(() => new RouteLeg(
            Guid.NewGuid(), 0, Geometry(), TimeSpan.FromMinutes(1), 10, [maneuver, maneuver]));
    }

    [Fact]
    public void Alternative_RequiresLegsAndExpiryAfterCreation()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => Alternative([], now, now.AddMinutes(1)));
        Assert.Throws<ArgumentException>(() => Alternative([Leg()], now, now.AddMinutes(-1)));
    }

    [Fact]
    public void Alternative_PreservesSummaryTradeoffsAndSourceVersions()
    {
        var route = Alternative([Leg()], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(1));

        Assert.Single(route.Legs);
        Assert.Contains(new Code("crowding"), route.PreferenceTradeoffs);
        Assert.Equal("2026.10", route.SourceVersions[new Code("osm")]);
        Assert.Single(route.AccessibilitySummary.SatisfiedHardConstraints);
    }

    private static RoutePlanRequest Request(
        IEnumerable<TravelMode> modes,
        IEnumerable<Code>? profiles = null) =>
        new(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50, 19)),
            new RouteEndpoint.Entity(Guid.NewGuid()),
            modes,
            new Dictionary<Code, ConstraintLevel>(),
            profiles ?? [new Code("fastest")],
            "pl-PL",
            new Dictionary<Code, string>());

    private static RouteAlternative Alternative(
        IEnumerable<RouteLeg> legs,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt) =>
        new(
            Guid.NewGuid(),
            new Code("easiest"),
            Geometry(),
            legs,
            TimeSpan.FromMinutes(12),
            850,
            [new CostComponent(new Code("time"), 12)],
            new AccessibilitySummary([new Code("stairs")], [new Code("surface")], [new Code("elevator")]),
            [new Code("crowding")],
            new ConfidenceAssessment(ConfidenceState.Supported, 0.8m, 4, createdAt),
            new Dictionary<Code, string> { [new Code("osm")] = "2026.10" },
            createdAt,
            expiresAt);

    private static RouteLeg Leg() =>
        new(Guid.NewGuid(), 0, Geometry(), TimeSpan.FromMinutes(12), 850, [Maneuver(0)]);

    private static Maneuver Maneuver(int position) =>
        new(position, new Code("continue"), new GeoCoordinate(50, 19), 20, TimeSpan.FromSeconds(15));

    private static SpatialGeometry Geometry() =>
        new(GeometryKind.Line, [new GeoCoordinate(50, 19), new GeoCoordinate(50.1m, 19.1m)]);
}
