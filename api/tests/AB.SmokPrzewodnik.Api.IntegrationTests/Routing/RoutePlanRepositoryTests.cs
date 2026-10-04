using AB.SmokPrzewodnik.Api.IntegrationTests.Infrastructure;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests.Routing;

[Collection(PostgreSqlCollection.Name)]
public sealed class RoutePlanRepositoryTests(PostgreSqlFixture database)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T10:00:00Z");

    [Fact]
    public async Task AddAndGet_RoundTripsSnapshotsAndOwnership()
    {
        using var scope = database.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRoutePlanRepository>();
        var accountId = Guid.NewGuid();
        var plan = CreatePlan(Guid.NewGuid(), accountId, Now, Now.AddMinutes(15));

        await repository.AddAsync(plan, CancellationToken.None);

        using var readScope = database.CreateScope();
        var stored = await readScope.ServiceProvider
            .GetRequiredService<IRoutePlanRepository>()
            .GetByIdAsync(plan.Id, CancellationToken.None);

        Assert.NotNull(stored);
        Assert.Equal(accountId, stored.AccountId);
        Assert.Equal(plan.Request.Locale, stored.Request.Locale);
        Assert.IsType<RouteEndpoint.Coordinate>(stored.Request.Origin);
        Assert.IsType<RouteEndpoint.Entity>(stored.Request.Destination);
        Assert.Equal(ConstraintLevel.MustAvoid, stored.Request.Constraints[new Code("stairs")]);
        var alternative = Assert.Single(stored.Alternatives);
        Assert.Equal(new Code("easiest"), alternative.LabelKey);
        Assert.Equal(GeometryKind.Line, alternative.Geometry.Kind);
        Assert.Equal("2026.10", alternative.SourceVersions[new Code("osm")]);
    }

    [Fact]
    public async Task DeleteExpired_RemovesOnlyExpiredPlans()
    {
        using var scope = database.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRoutePlanRepository>();
        var expired = CreatePlan(Guid.NewGuid(), null, Now.AddHours(-2), Now.AddHours(-1));
        var current = CreatePlan(Guid.NewGuid(), null, Now, Now.AddMinutes(15));
        await repository.AddAsync(expired, CancellationToken.None);
        await repository.AddAsync(current, CancellationToken.None);

        var deleted = await repository.DeleteExpiredAsync(Now, CancellationToken.None);

        Assert.Equal(1, deleted);
        Assert.Null(await repository.GetByIdAsync(expired.Id, CancellationToken.None));
        Assert.NotNull(await repository.GetByIdAsync(current.Id, CancellationToken.None));
    }

    private static RoutePlan CreatePlan(
        Guid id,
        Guid? accountId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        var request = new RoutePlanRequest(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.0617m, 19.9373m)),
            new RouteEndpoint.Entity(Guid.NewGuid()),
            [TravelMode.Walk],
            new Dictionary<Code, ConstraintLevel> { [new Code("stairs")] = ConstraintLevel.MustAvoid },
            [new Code("easiest")],
            "pl-PL",
            new Dictionary<Code, string> { [new Code("osm")] = "2026.10" });
        var geometry = new SpatialGeometry(
            GeometryKind.Line,
            [new GeoCoordinate(50.0617m, 19.9373m), new GeoCoordinate(50.062m, 19.94m)]);
        var leg = new RouteLeg(
            Guid.NewGuid(),
            0,
            geometry,
            TimeSpan.FromMinutes(5),
            400,
            [new Maneuver(0, new Code("continue"), geometry.Coordinates[0], 400, TimeSpan.FromMinutes(5))]);
        var alternative = new RouteAlternative(
            Guid.NewGuid(),
            new Code("easiest"),
            geometry,
            [leg],
            TimeSpan.FromMinutes(5),
            400,
            [new CostComponent(new Code("time"), 5)],
            new AccessibilitySummary([new Code("stairs")], [], []),
            [],
            new ConfidenceAssessment(ConfidenceState.Supported, 0.9m, 2, createdAt),
            new Dictionary<Code, string> { [new Code("osm")] = "2026.10" },
            createdAt,
            expiresAt.AddMinutes(15));

        return new RoutePlan(id, accountId, request, [alternative], createdAt, expiresAt);
    }
}
