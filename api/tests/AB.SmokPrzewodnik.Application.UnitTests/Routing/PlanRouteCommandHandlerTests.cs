using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Application.Routing.Commands;
using AB.SmokPrzewodnik.Application.Routing.Dtos;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Routing;

public sealed class PlanRouteCommandHandlerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task Handle_ResolvesEntityAndPersistsOwnedPlan()
    {
        var accountId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var entity = Spatial(entityId, new GeoCoordinate(50.07m, 19.95m));
        var planner = new FakePlanner([Alternative([])]);
        var plans = new FakeRoutePlanRepository();
        var handler = new PlanRouteCommandHandler(planner, plans, new FakeSpatialRepository(entity), new FixedTimeProvider(Now));

        var response = await handler.Handle(Command(
            new RouteEndpointDto(null, null, entityId), accountId), CancellationToken.None);

        Assert.NotNull(plans.Added);
        Assert.Equal(accountId, plans.Added.AccountId);
        Assert.Equal(response.RoutePlanId, plans.Added.Id);
        var origin = Assert.IsType<RouteEndpoint.Entity>(plans.Added.Request.Origin);
        Assert.Equal(entityId, origin.EntityId);
        var providerOrigin = Assert.IsType<RouteEndpoint.Coordinate>(planner.Request!.Origin);
        Assert.Equal(entity.Geometry.Coordinates[0], providerOrigin.Value);
    }

    [Fact]
    public async Task Handle_MissingEntity_ThrowsNotFoundAndDoesNotPersist()
    {
        var plans = new FakeRoutePlanRepository();
        var handler = new PlanRouteCommandHandler(
            new FakePlanner([Alternative([])]), plans, new FakeSpatialRepository(null), new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<RouteEndpointNotFoundException>(() => handler.Handle(
            Command(new RouteEndpointDto(null, null, Guid.NewGuid()), null), CancellationToken.None));

        Assert.Null(plans.Added);
    }

    [Fact]
    public async Task Handle_RejectsCandidatesWithUnverifiedMustAvoidConstraint()
    {
        var plans = new FakeRoutePlanRepository();
        var handler = new PlanRouteCommandHandler(
            new FakePlanner([Alternative([new Code("stairs")])]),
            plans,
            new FakeSpatialRepository(null),
            new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<NoViableRouteException>(() => handler.Handle(
            Command(new RouteEndpointDto(50.06m, 19.93m, null), null,
                new Dictionary<string, ConstraintLevel> { ["stairs"] = ConstraintLevel.MustAvoid }),
            CancellationToken.None));

        Assert.Null(plans.Added);
    }

    private static PlanRouteCommand Command(
        RouteEndpointDto origin,
        Guid? accountId,
        IReadOnlyDictionary<string, ConstraintLevel>? constraints = null) =>
        new(
            origin,
            new RouteEndpointDto(50.08m, 19.96m, null),
            [TravelMode.Walk],
            constraints ?? new Dictionary<string, ConstraintLevel>(),
            ["fastest"],
            "pl-PL",
            new Dictionary<string, string>(),
            accountId);

    private static RouteAlternative Alternative(IEnumerable<Code> unknowns) => new(
        Guid.NewGuid(), new Code("fastest"),
        new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50.06m, 19.93m), new GeoCoordinate(50.08m, 19.96m)]),
        [new RouteLeg(Guid.NewGuid(), 0,
            new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50.06m, 19.93m), new GeoCoordinate(50.08m, 19.96m)]),
            TimeSpan.FromMinutes(5), 500m, [])],
        TimeSpan.FromMinutes(5), 500m, [], new AccessibilitySummary([], unknowns, []), [],
        new ConfidenceAssessment(ConfidenceState.Unverified, 0m, 0, Now),
        new Dictionary<Code, string> { [new Code("valhalla")] = "3.x" }, Now, Now.AddMinutes(15));

    private static SpatialEntity Spatial(Guid id, GeoCoordinate coordinate) => new(
        id, Guid.NewGuid(), EntityKind.Place, new SpatialGeometry(GeometryKind.Point, [coordinate]),
        LifecycleState.Active, new ConfidenceAssessment(ConfidenceState.Supported, 1m, 1, Now), Now, Now);

    private sealed class FakePlanner(IReadOnlyList<RouteAlternative> alternatives) : IRoutePlanner
    {
        public RoutePlanRequest? Request { get; private set; }
        public Task<IReadOnlyList<RouteAlternative>> PlanAsync(RoutePlanRequest request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(alternatives);
        }
    }

    private sealed class FakeRoutePlanRepository : IRoutePlanRepository
    {
        public RoutePlan? Added { get; private set; }
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RoutePlan?>(null);
        public Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken) { Added = routePlan; return Task.CompletedTask; }
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
    }

    private sealed class FakeSpatialRepository(SpatialEntity? entity) : ISpatialEntityRepository
    {
        public Task<SpatialEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(entity);
        public Task<CursorPage<SpatialEntity>> FindAsync(SpatialEntityCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
            Task.FromResult(new CursorPage<SpatialEntity>([], null));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
