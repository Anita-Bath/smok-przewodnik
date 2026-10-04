using System.Net;
using System.Net.Http.Json;
using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
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

public sealed class NavigationProgressAndEventsTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task TokenAuthorizedProgress_CanBeRecoveredBySequence()
    {
        var session = Session();
        var repository = new InMemorySessionRepository(session);
        await using var application = CreateApplication(session, repository);
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("X-Navigation-Token", "raw-token");

        var progressResponse = await client.PostAsJsonAsync(
            $"/v1/navigation/sessions/{session.Id}/progress",
            new UpdateNavigationProgressRequest(
                new GeoCoordinateDto(50.01m, 19.01m), 5m, 0m, Now, null, null));
        var eventsResponse = await client.GetAsync(
            $"/v1/navigation/sessions/{session.Id}/events?afterSequence=0");

        Assert.True(progressResponse.StatusCode == HttpStatusCode.OK,
            await progressResponse.Content.ReadAsStringAsync());
        Assert.True(eventsResponse.StatusCode == HttpStatusCode.OK,
            await eventsResponse.Content.ReadAsStringAsync());
        var events = await eventsResponse.Content.ReadFromJsonAsync<NavigationEventsResponse>();
        Assert.NotNull(events);
        Assert.Equal(1, events.LatestSequence);
        Assert.Equal("Arrival", Assert.Single(events.Items).EventType);
    }

    [Fact]
    public async Task Progress_WithoutCredential_ReturnsUnauthorized()
    {
        var session = Session();
        await using var application = CreateApplication(session, new InMemorySessionRepository(session));
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/v1/navigation/sessions/{session.Id}/progress",
            new UpdateNavigationProgressRequest(
                new GeoCoordinateDto(50.01m, 19.01m), null, null, Now, null, null));

        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized,
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task TokenForAnotherSession_ReturnsForbidden()
    {
        var session = Session();
        await using var application = CreateApplication(session, new InMemorySessionRepository(session));
        using var client = application.CreateClient();
        client.DefaultRequestHeaders.Add("X-Navigation-Token", "raw-token");

        var response = await client.GetAsync(
            $"/v1/navigation/sessions/{Guid.NewGuid()}/events?afterSequence=0");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        NavigationSession session,
        InMemorySessionRepository repository) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Pagination:SigningKey", "integration-test-signing-key");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<INavigationSessionRepository>();
                services.AddSingleton<INavigationSessionRepository>(repository);
                services.RemoveAll<INavigationTokenService>();
                services.AddSingleton<INavigationTokenService>(new TokenService(session));
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            });
        });

    private static NavigationSession Session()
    {
        var geometry = new SpatialGeometry(GeometryKind.Line,
            [new GeoCoordinate(50m, 19m), new GeoCoordinate(50.01m, 19.01m)]);
        var route = new RouteAlternative(Guid.NewGuid(), new Code("fastest"), geometry,
            [new RouteLeg(Guid.NewGuid(), 0, geometry, TimeSpan.FromMinutes(5), 500m, [])],
            TimeSpan.FromMinutes(5), 500m, [], new AccessibilitySummary([], [], []), [],
            new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, Now), new Dictionary<Code, string>(), Now, Now.AddHours(1));
        var request = new RoutePlanRequest(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.01m, 19.01m)), [TravelMode.Walk],
            new Dictionary<Code, ConstraintLevel>(), [new Code("fastest")], "pl-PL", new Dictionary<Code, string>());
        return new NavigationSession(Guid.NewGuid(), "token-hash", null, route, request, Now.AddMinutes(-1), Now.AddHours(1));
    }

    private sealed class InMemorySessionRepository(NavigationSession session) : INavigationSessionRepository
    {
        private readonly List<NavigationEvent> _events = [];
        public Task AddAsync(NavigationSession value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(NavigationSession value, IReadOnlyCollection<NavigationEvent> events, CancellationToken cancellationToken)
        { _events.AddRange(events); return Task.CompletedTask; }
        public Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(Guid sessionId, long afterSequence, int limit, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<NavigationEvent>>(_events.Where(item => item.Sequence > afterSequence).OrderBy(item => item.Sequence).Take(limit).ToArray());
        public Task DeleteAsync(NavigationSession value, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
        public Task<NavigationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NavigationSession?>(id == session.Id ? session : null);
        public Task<CursorPage<NavigationSession>> FindAsync(NavigationSessionCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
            Task.FromResult(new CursorPage<NavigationSession>([session], null));
    }

    private sealed class TokenService(NavigationSession session) : INavigationTokenService
    {
        public IssuedNavigationToken IssueToken() => new("raw-token", "token-hash");
        public Task<NavigationSessionDto?> GetSessionAsync(string token, CancellationToken cancellationToken) =>
            Task.FromResult<NavigationSessionDto?>(token == "raw-token"
                ? new NavigationSessionDto(session.Id, session.Hash, session.ExpiresAt, session.AccountId)
                : null);
        public Task<bool> IsActiveAndOwnedByAsync(Guid sessionId, Guid accountId, CancellationToken cancellationToken) => Task.FromResult(false);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
