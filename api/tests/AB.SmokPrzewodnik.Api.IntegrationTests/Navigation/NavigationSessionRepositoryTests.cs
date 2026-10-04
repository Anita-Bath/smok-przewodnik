using AB.SmokPrzewodnik.Api.IntegrationTests.Infrastructure;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests.Navigation;

[Collection(PostgreSqlCollection.Name)]
public sealed class NavigationSessionRepositoryTests(PostgreSqlFixture fixture)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task AddUpdateAndReadEvents_RoundTripsSnapshotsAndSequence()
    {
        var session = Session();
        using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<INavigationSessionRepository>();
            await repository.AddAsync(session, CancellationToken.None);
            var progress = new NavigationProgress(new GeoCoordinate(50m, 19m), 5m, 90m, Now, null, null, 50m, false);
            session.TryUpdateProgress(progress, Now);
            var item = Event(session, Now);
            await repository.UpdateAsync(session, [item], CancellationToken.None);
        }

        using var verificationScope = fixture.CreateScope();
        var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<INavigationSessionRepository>();
        var loaded = await verificationRepository.GetByIdAsync(session.Id, CancellationToken.None);
        var events = await verificationRepository.GetEventsAfterAsync(session.Id, 0, 100, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(session.ActiveRoute.Id, loaded.ActiveRoute.Id);
        Assert.Equal(2, loaded.NextEventSequence);
        Assert.Equal(1, Assert.Single(events).Sequence);
    }

    [Fact]
    public async Task ConcurrentUpdates_DoNotPersistDuplicateSequence()
    {
        var session = Session();
        using (var setupScope = fixture.CreateScope())
        {
            await setupScope.ServiceProvider.GetRequiredService<INavigationSessionRepository>()
                .AddAsync(session, CancellationToken.None);
        }

        using var firstScope = fixture.CreateScope();
        using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<INavigationSessionRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<INavigationSessionRepository>();
        var first = (await firstRepository.GetByIdAsync(session.Id, CancellationToken.None))!;
        var second = (await secondRepository.GetByIdAsync(session.Id, CancellationToken.None))!;
        first.TryUpdateProgress(new NavigationProgress(new GeoCoordinate(50m, 19m), null, null, Now, null, null, 20m, false), Now);
        second.TryUpdateProgress(new NavigationProgress(new GeoCoordinate(50m, 19m), null, null, Now.AddSeconds(1), null, null, 10m, false), Now.AddSeconds(1));

        var results = await Task.WhenAll(
            Capture(() => firstRepository.UpdateAsync(first, [Event(first, Now)], CancellationToken.None)),
            Capture(() => secondRepository.UpdateAsync(second, [Event(second, Now.AddSeconds(1))], CancellationToken.None)));

        Assert.Single(results, exception => exception is null);
        Assert.Single(results, exception => exception is NavigationConcurrencyException);
        using var verificationScope = fixture.CreateScope();
        var events = await verificationScope.ServiceProvider.GetRequiredService<INavigationSessionRepository>()
            .GetEventsAfterAsync(session.Id, 0, 100, CancellationToken.None);
        Assert.Equal([1L], events.Select(item => item.Sequence).ToArray());
    }

    private static async Task<Exception?> Capture(Func<Task> action)
    {
        try { await action(); return null; }
        catch (Exception exception) { return exception; }
    }

    private static NavigationEvent Event(NavigationSession session, DateTimeOffset at) => session.AppendEvent(
        NavigationEventType.Landmark, NavigationUrgency.Informational, null, null, new Code("test"),
        10m, new Dictionary<string, string>(), [new Code("audio")], at);

    private static NavigationSession Session()
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
        return new NavigationSession(Guid.NewGuid(), $"hash-{Guid.NewGuid():N}", null, route, request, Now.AddMinutes(-1), Now.AddHours(1));
    }
}
