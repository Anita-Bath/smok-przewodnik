using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
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

public sealed class StartNavigationSessionCommandHandlerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task Handle_CreatesAnonymousSessionAndReturnsRawToken()
    {
        var plan = Plan(null);
        var sessions = new FakeSessionRepository();
        var handler = Handler(plan, sessions);

        var response = await handler.Handle(
            new StartNavigationSessionCommand(plan.Id, plan.Alternatives[0].Id, null), CancellationToken.None);

        Assert.Equal("raw-token", response.NavigationToken);
        Assert.Equal("token-hash", sessions.Added?.Hash);
        Assert.Null(sessions.Added?.AccountId);
        Assert.Equal(plan.Alternatives[0].Id, response.Route.Id);
    }

    [Fact]
    public async Task Handle_RejectsAnotherAccountsPlan()
    {
        var sessions = new FakeSessionRepository();
        var plan = Plan(Guid.NewGuid());
        var handler = Handler(plan, sessions);

        await Assert.ThrowsAsync<RoutePlanAccessDeniedException>(() => handler.Handle(
            new StartNavigationSessionCommand(plan.Id, plan.Alternatives[0].Id, Guid.NewGuid()), CancellationToken.None));

        Assert.Null(sessions.Added);
    }

    [Fact]
    public async Task Handle_RejectsExpiredPlan()
    {
        var sessions = new FakeSessionRepository();
        var plan = Plan(null, Now.AddSeconds(-1));
        var handler = Handler(plan, sessions);

        await Assert.ThrowsAsync<RoutePlanExpiredException>(() => handler.Handle(
            new StartNavigationSessionCommand(plan.Id, plan.Alternatives[0].Id, null), CancellationToken.None));

        Assert.Null(sessions.Added);
    }

    private static StartNavigationSessionCommandHandler Handler(RoutePlan plan, FakeSessionRepository sessions) =>
        new(new FakePlanRepository(plan), sessions, new FakeTokenService(), new FixedTimeProvider(Now));

    private static RoutePlan Plan(Guid? accountId, DateTimeOffset? expiresAt = null)
    {
        var alternative = Alternative(expiresAt ?? Now.AddMinutes(15));
        return new RoutePlan(Guid.NewGuid(), accountId, Request(), [alternative], Now.AddMinutes(-1), alternative.ExpiresAt);
    }

    private static RoutePlanRequest Request() => new(
        new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
        new RouteEndpoint.Coordinate(new GeoCoordinate(51m, 20m)),
        [TravelMode.Walk], new Dictionary<Code, ConstraintLevel>(), [new Code("fastest")], "pl-PL",
        new Dictionary<Code, string>());

    private static RouteAlternative Alternative(DateTimeOffset expiresAt) => new(
        Guid.NewGuid(), new Code("fastest"),
        new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(51m, 20m)]),
        [new RouteLeg(Guid.NewGuid(), 0,
            new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(51m, 20m)]),
            TimeSpan.FromMinutes(1), 10m, [])],
        TimeSpan.FromMinutes(1), 10m, [], new AccessibilitySummary([], [], []), [],
        new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, Now), new Dictionary<Code, string>(),
        Now.AddMinutes(-1), expiresAt);

    private sealed class FakePlanRepository(RoutePlan plan) : IRoutePlanRepository
    {
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RoutePlan?>(id == plan.Id ? plan : null);
        public Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
    }

    private sealed class FakeSessionRepository : INavigationSessionRepository
    {
        public NavigationSession? Added { get; private set; }
        public Task AddAsync(NavigationSession session, CancellationToken cancellationToken) { Added = session; return Task.CompletedTask; }
        public Task UpdateAsync(NavigationSession session, IReadOnlyCollection<NavigationEvent> events, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(Guid sessionId, long afterSequence, int limit, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<NavigationEvent>>([]);
        public Task DeleteAsync(NavigationSession session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<NavigationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NavigationSession?>(null);
        public Task<CursorPage<NavigationSession>> FindAsync(NavigationSessionCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
            Task.FromResult(new CursorPage<NavigationSession>([], null));
    }

    private sealed class FakeTokenService : INavigationTokenService
    {
        public IssuedNavigationToken IssueToken() => new("raw-token", "token-hash");
        public Task<NavigationSessionDto?> GetSessionAsync(string token, CancellationToken cancellationToken) => Task.FromResult<NavigationSessionDto?>(null);
        public Task<bool> IsActiveAndOwnedByAsync(Guid sessionId, Guid accountId, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
