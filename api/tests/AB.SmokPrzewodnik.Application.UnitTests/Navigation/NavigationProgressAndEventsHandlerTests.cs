using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Navigation.Commands;
using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Navigation.Queries;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Navigation;

public sealed class UpdateNavigationProgressCommandHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesProgressAndPersistsGeneratedEvent()
    {
        var session = NavigationTestData.Session();
        var repository = new FakeNavigationRepository(session);
        var handler = new UpdateNavigationProgressCommandHandler(
            repository,
            new FixedMatcher(new NavigationProgressMatch(new GeoCoordinate(50.001m, 19.001m), 100m, false, null)),
            new FixedTimeProvider(NavigationTestData.Now));

        var response = await handler.Handle(new UpdateNavigationProgressCommand(
            session.Id,
            new UpdateNavigationProgressRequest(
                new GeoCoordinateDto(50m, 19m), 5m, 90m, NavigationTestData.Now, null, null)),
            CancellationToken.None);

        Assert.False(response.WasIgnored);
        Assert.NotNull(repository.Updated);
        Assert.NotNull(session.LatestProgress);
    }

    [Fact]
    public async Task Handle_OlderObservation_IsIgnoredWithoutSaving()
    {
        var session = NavigationTestData.Session();
        session.TryUpdateProgress(new NavigationProgress(
            new GeoCoordinate(50m, 19m), null, null, NavigationTestData.Now, null, null, 50m, false), NavigationTestData.Now);
        var repository = new FakeNavigationRepository(session);
        var handler = new UpdateNavigationProgressCommandHandler(
            repository,
            new FixedMatcher(new NavigationProgressMatch(new GeoCoordinate(50m, 19m), 50m, false, null)),
            new FixedTimeProvider(NavigationTestData.Now));

        var response = await handler.Handle(new UpdateNavigationProgressCommand(
            session.Id,
            new UpdateNavigationProgressRequest(
                new GeoCoordinateDto(50m, 19m), null, null, NavigationTestData.Now.AddSeconds(-1), null, null)),
            CancellationToken.None);

        Assert.True(response.WasIgnored);
        Assert.Null(repository.Updated);
    }

    private sealed class FixedMatcher(NavigationProgressMatch result) : INavigationProgressMatcher
    {
        public NavigationProgressMatch Match(RouteAlternative route, NavigationProgress progress) => result;
    }
}

public sealed class GetNavigationEventsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsEventsAfterSequenceAndLatestSequence()
    {
        var session = NavigationTestData.Session();
        var first = NavigationTestData.Event(session, NavigationTestData.Now);
        var second = NavigationTestData.Event(session, NavigationTestData.Now.AddSeconds(1));
        var repository = new FakeNavigationRepository(session, [second]);
        var handler = new GetNavigationEventsQueryHandler(repository, new FixedTimeProvider(NavigationTestData.Now));

        var response = await handler.Handle(new GetNavigationEventsQuery(session.Id, first.Sequence), CancellationToken.None);

        Assert.Equal(first.Sequence, repository.AfterSequence);
        Assert.Equal(second.Sequence, Assert.Single(response.Items).Sequence);
        Assert.Equal(second.Sequence, response.LatestSequence);
        Assert.False(response.HasMore);
    }

    [Fact]
    public async Task Handle_NegativeSequence_IsRejected()
    {
        var session = NavigationTestData.Session();
        var handler = new GetNavigationEventsQueryHandler(
            new FakeNavigationRepository(session), new FixedTimeProvider(NavigationTestData.Now));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            handler.Handle(new GetNavigationEventsQuery(session.Id, -1), CancellationToken.None));
    }
}

internal sealed class FakeNavigationRepository(
    NavigationSession session,
    IReadOnlyList<NavigationEvent>? events = null) : INavigationSessionRepository
{
    public NavigationSession? Updated { get; private set; }
    public long? AfterSequence { get; private set; }
    public Task AddAsync(NavigationSession value, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task UpdateAsync(NavigationSession value, IReadOnlyCollection<NavigationEvent> newEvents, CancellationToken cancellationToken)
    { Updated = value; return Task.CompletedTask; }
    public Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(Guid sessionId, long afterSequence, int limit, CancellationToken cancellationToken)
    { AfterSequence = afterSequence; return Task.FromResult(events ?? (IReadOnlyList<NavigationEvent>)[]); }
    public Task DeleteAsync(NavigationSession value, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
    public Task<NavigationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NavigationSession?>(id == session.Id ? session : null);
    public Task<CursorPage<NavigationSession>> FindAsync(NavigationSessionCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
        Task.FromResult(new CursorPage<NavigationSession>([session], null));
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal static class NavigationTestData
{
    public static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    public static NavigationSession Session()
    {
        var geometry = new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(50.01m, 19.01m)]);
        var route = new RouteAlternative(Guid.NewGuid(), new Code("fastest"), geometry,
            [new RouteLeg(Guid.NewGuid(), 0, geometry, TimeSpan.FromMinutes(5), 500m, [])],
            TimeSpan.FromMinutes(5), 500m, [], new AccessibilitySummary([], [], []), [],
            new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, Now), new Dictionary<Code, string>(), Now, Now.AddHours(1));
        var request = new RoutePlanRequest(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.01m, 19.01m)), [TravelMode.Walk],
            new Dictionary<Code, ConstraintLevel>(), [new Code("fastest")], "pl-PL", new Dictionary<Code, string>());
        return new NavigationSession(Guid.NewGuid(), "hash", null, route, request, Now.AddMinutes(-1), Now.AddHours(1));
    }

    public static NavigationEvent Event(NavigationSession session, DateTimeOffset at) => session.AppendEvent(
        NavigationEventType.Landmark, NavigationUrgency.Informational, null, null, new Code("test"),
        100m, new Dictionary<string, string>(), [new Code("audio")], at);
}
