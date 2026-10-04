using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Routing;

public sealed class RoutePlanTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T10:00:00Z");

    [Fact]
    public void Constructor_RejectsEmptyAlternatives()
    {
        Assert.Throws<ArgumentException>(() => new RoutePlan(
            Guid.NewGuid(),
            null,
            CreateRequest(),
            [],
            Now,
            Now.AddMinutes(15)));
    }

    [Fact]
    public void Constructor_RejectsEmptyIdentifier()
    {
        Assert.Throws<ArgumentException>(() => new RoutePlan(
            Guid.Empty,
            null,
            CreateRequest(),
            [CreateAlternative(Now.AddMinutes(30))],
            Now,
            Now.AddMinutes(15)));
    }

    [Fact]
    public void Constructor_RejectsAlternativeExpiringBeforePlan()
    {
        Assert.Throws<ArgumentException>(() => new RoutePlan(
            Guid.NewGuid(),
            null,
            CreateRequest(),
            [CreateAlternative(Now.AddMinutes(10))],
            Now,
            Now.AddMinutes(15)));
    }

    [Fact]
    public void Constructor_CopiesAlternatives()
    {
        var alternatives = new List<RouteAlternative> { CreateAlternative(Now.AddMinutes(30)) };
        var plan = new RoutePlan(
            Guid.NewGuid(),
            null,
            CreateRequest(),
            alternatives,
            Now,
            Now.AddMinutes(15));

        alternatives.Clear();

        Assert.Single(plan.Alternatives);
        Assert.True(Assert.IsAssignableFrom<ICollection<RouteAlternative>>(plan.Alternatives).IsReadOnly);
    }

    [Fact]
    public void FindAlternative_ReturnsOnlyOwnedAlternative()
    {
        var alternative = CreateAlternative(Now.AddMinutes(30));
        var plan = new RoutePlan(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CreateRequest(),
            [alternative],
            Now,
            Now.AddMinutes(15));

        Assert.Same(alternative, plan.FindAlternative(alternative.Id));
        Assert.Null(plan.FindAlternative(Guid.NewGuid()));
    }

    [Fact]
    public void IsExpired_UsesProvidedTime()
    {
        var plan = new RoutePlan(
            Guid.NewGuid(),
            null,
            CreateRequest(),
            [CreateAlternative(Now.AddMinutes(30))],
            Now,
            Now.AddMinutes(15));

        Assert.False(plan.IsExpired(Now.AddMinutes(14)));
        Assert.True(plan.IsExpired(Now.AddMinutes(15)));
        Assert.True(plan.IsExpired(Now.AddMinutes(16)));
    }

    private static RoutePlanRequest CreateRequest() =>
        new(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.0617m, 19.9373m)),
            new RouteEndpoint.Entity(Guid.NewGuid()),
            [TravelMode.Walk],
            new Dictionary<Code, ConstraintLevel>
            {
                [new Code("stairs")] = ConstraintLevel.MustAvoid
            },
            [new Code("easiest")],
            "pl-PL",
            new Dictionary<Code, string>
            {
                [new Code("osm")] = "2026.10"
            });

    private static RouteAlternative CreateAlternative(DateTimeOffset expiresAt)
    {
        var geometry = new SpatialGeometry(
            GeometryKind.Line,
            [new GeoCoordinate(50.0617m, 19.9373m), new GeoCoordinate(50.062m, 19.94m)]);
        var leg = new RouteLeg(
            Guid.NewGuid(),
            0,
            geometry,
            TimeSpan.FromMinutes(5),
            400,
            [new Maneuver(0, new Code("continue"), new GeoCoordinate(50.0617m, 19.9373m), 400, TimeSpan.FromMinutes(5))]);

        return new RouteAlternative(
            Guid.NewGuid(),
            new Code("easiest"),
            geometry,
            [leg],
            TimeSpan.FromMinutes(5),
            400,
            [new CostComponent(new Code("time"), 5)],
            new AccessibilitySummary([new Code("stairs")], [], []),
            [],
            new ConfidenceAssessment(ConfidenceState.Supported, 0.9m, 2, Now),
            new Dictionary<Code, string> { [new Code("osm")] = "2026.10" },
            Now,
            expiresAt);
    }
}
