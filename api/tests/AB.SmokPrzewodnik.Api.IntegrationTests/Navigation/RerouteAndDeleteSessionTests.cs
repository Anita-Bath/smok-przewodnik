using System.Net;
using System.Net.Http.Json;
using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests.Navigation;

public sealed class RerouteAndDeleteSessionTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task AuthorizedSession_CanConfirmRerouteAndThenDelete()
    {
        var session = Session();
        var replacement = Route("easiest", 600m);
        var plan = new RoutePlan(Guid.NewGuid(), null, Request(), [replacement], Now, Now.AddMinutes(15));
        var sessions = new SessionRepository(session);
        await using var application = CreateApplication(session, sessions, plan);
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("X-Navigation-Token", "raw-token");

        var reroute = await client.PostAsJsonAsync(
            $"/v1/navigation/sessions/{session.Id}/reroute",
            new RerouteNavigationSessionRequest(plan.Id, replacement.Id));
        var delete = await client.DeleteAsync($"/v1/navigation/sessions/{session.Id}");

        Assert.Equal(HttpStatusCode.OK, reroute.StatusCode);
        var response = await reroute.Content.ReadFromJsonAsync<RerouteNavigationSessionResponse>();
        Assert.NotNull(response);
        Assert.Equal(replacement.Id, response.Route.Id);
        Assert.Equal(1, response.LatestSequence);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.True(sessions.Deleted);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        NavigationSession session,
        SessionRepository sessions,
        RoutePlan plan) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Pagination:SigningKey", "integration-test-signing-key");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<INavigationSessionRepository>();
                services.AddSingleton<INavigationSessionRepository>(sessions);
                services.RemoveAll<IRoutePlanRepository>();
                services.AddSingleton<IRoutePlanRepository>(new PlanRepository(plan));
                services.RemoveAll<INavigationTokenService>();
                services.AddSingleton<INavigationTokenService>(new TokenService(session));
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            });
        });

    private static NavigationSession Session() => new(
        Guid.NewGuid(), "token-hash", null, Route("fastest", 500m), Request(),
        Now.AddMinutes(-1), Now.AddHours(1));

    private static RoutePlanRequest Request() => new(
        new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
        new RouteEndpoint.Coordinate(new GeoCoordinate(50.01m, 19.01m)), [TravelMode.Walk],
        new Dictionary<Code, ConstraintLevel>(), [new Code("fastest")], "pl-PL", new Dictionary<Code, string>());

    private static RouteAlternative Route(string label, decimal distance)
    {
        var geometry = new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(50.01m, 19.01m)]);
        return new RouteAlternative(Guid.NewGuid(), new Code(label), geometry,
            [new RouteLeg(Guid.NewGuid(), 0, geometry, TimeSpan.FromMinutes(5), distance, [])],
            TimeSpan.FromMinutes(5), distance, [], new AccessibilitySummary([], [], []), [],
            new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, Now), new Dictionary<Code, string>(), Now, Now.AddMinutes(15));
    }

    private sealed class SessionRepository(NavigationSession session) : INavigationSessionRepository
    {
        public bool Deleted { get; private set; }
        public Task AddAsync(NavigationSession value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(NavigationSession value, IReadOnlyCollection<NavigationEvent> events, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(Guid sessionId, long afterSequence, int limit, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<NavigationEvent>>([]);
        public Task DeleteAsync(NavigationSession value, CancellationToken cancellationToken) { Deleted = true; return Task.CompletedTask; }
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<NavigationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NavigationSession?>(!Deleted && id == session.Id ? session : null);
        public Task<CursorPage<NavigationSession>> FindAsync(NavigationSessionCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
            Task.FromResult(new CursorPage<NavigationSession>(Deleted ? [] : [session], null));
    }

    private sealed class PlanRepository(RoutePlan plan) : IRoutePlanRepository
    {
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RoutePlan?>(id == plan.Id ? plan : null);
        public Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
    }

    private sealed class TokenService(NavigationSession session) : INavigationTokenService
    {
        public IssuedNavigationToken IssueToken() => new("raw-token", "token-hash");
        public Task<NavigationSessionDto?> GetSessionAsync(string token, CancellationToken cancellationToken) =>
            Task.FromResult<NavigationSessionDto?>(new NavigationSessionDto(session.Id, session.Hash, session.ExpiresAt, session.AccountId));
        public Task<bool> IsActiveAndOwnedByAsync(Guid sessionId, Guid accountId, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
