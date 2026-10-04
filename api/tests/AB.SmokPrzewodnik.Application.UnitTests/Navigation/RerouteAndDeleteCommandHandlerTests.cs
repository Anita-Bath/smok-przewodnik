using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Navigation.Commands;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Navigation;

public sealed class RerouteNavigationSessionCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReplacesRouteAndAppendsSingleEvent()
    {
        var session = NavigationTestData.Session();
        var route = Route();
        var plan = Plan(route, session.AccountId);
        var sessions = new Repository(session);
        var handler = new RerouteNavigationSessionCommandHandler(
            sessions, new PlanRepository(plan), new FixedTimeProvider(NavigationTestData.Now));

        var response = await handler.Handle(
            new RerouteNavigationSessionCommand(session.Id, plan.Id, route.Id), CancellationToken.None);

        Assert.Equal(route.Id, response.Route.Id);
        Assert.Equal(1, response.LatestSequence);
        Assert.Equal(NavigationEventType.Reroute, Assert.Single(sessions.Events).EventType);
    }

    [Fact]
    public async Task Handle_AlreadyActiveRoute_IsIdempotent()
    {
        var session = NavigationTestData.Session();
        var plan = Plan(session.ActiveRoute, session.AccountId);
        var sessions = new Repository(session);
        var handler = new RerouteNavigationSessionCommandHandler(
            sessions, new PlanRepository(plan), new FixedTimeProvider(NavigationTestData.Now));

        var response = await handler.Handle(
            new RerouteNavigationSessionCommand(session.Id, plan.Id, session.ActiveRoute.Id), CancellationToken.None);

        Assert.Equal(0, response.LatestSequence);
        Assert.Empty(sessions.Events);
        Assert.False(sessions.WasUpdated);
    }

    private static RoutePlan Plan(RouteAlternative route, Guid? accountId) => new(
        Guid.NewGuid(), accountId, Request(), [route], NavigationTestData.Now, NavigationTestData.Now.AddMinutes(15));

    private static RouteAlternative Route()
    {
        var geometry = new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(50.02m, 19.02m)]);
        return new RouteAlternative(Guid.NewGuid(), new Code("easiest"), geometry,
            [new RouteLeg(Guid.NewGuid(), 0, geometry, TimeSpan.FromMinutes(6), 600m, [])],
            TimeSpan.FromMinutes(6), 600m, [], new AccessibilitySummary([], [], []), [],
            new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, NavigationTestData.Now),
            new Dictionary<Code, string>(), NavigationTestData.Now, NavigationTestData.Now.AddMinutes(15));
    }

    private static RoutePlanRequest Request() => new(
        new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
        new RouteEndpoint.Coordinate(new GeoCoordinate(50.02m, 19.02m)), [TravelMode.Walk],
        new Dictionary<Code, ConstraintLevel>(), [new Code("easiest")], "pl-PL", new Dictionary<Code, string>());

    private sealed class PlanRepository(RoutePlan plan) : IRoutePlanRepository
    {
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RoutePlan?>(id == plan.Id ? plan : null);
        public Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
    }

    internal sealed class Repository(NavigationSession session) : INavigationSessionRepository
    {
        public List<NavigationEvent> Events { get; } = [];
        public bool WasUpdated { get; private set; }
        public bool WasDeleted { get; private set; }
        public Task AddAsync(NavigationSession value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(NavigationSession value, IReadOnlyCollection<NavigationEvent> events, CancellationToken cancellationToken)
        { WasUpdated = true; Events.AddRange(events); return Task.CompletedTask; }
        public Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(Guid sessionId, long afterSequence, int limit, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<NavigationEvent>>(Events);
        public Task DeleteAsync(NavigationSession value, CancellationToken cancellationToken) { WasDeleted = true; return Task.CompletedTask; }
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<NavigationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NavigationSession?>(id == session.Id ? session : null);
        public Task<CursorPage<NavigationSession>> FindAsync(NavigationSessionCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
            Task.FromResult(new CursorPage<NavigationSession>([session], null));
    }
}

public sealed class DeleteNavigationSessionCommandHandlerTests
{
    [Fact]
    public async Task Handle_DeletesExistingSession()
    {
        var session = NavigationTestData.Session();
        var repository = new RerouteNavigationSessionCommandHandlerTests.Repository(session);
        var handler = new DeleteNavigationSessionCommandHandler(repository);

        await handler.Handle(new DeleteNavigationSessionCommand(session.Id), CancellationToken.None);

        Assert.True(repository.WasDeleted);
    }
}
