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

public sealed class StartNavigationSessionEndpointTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task StartSession_AnonymousRequest_ReturnsCreatedWithRawToken()
    {
        var plan = Plan();
        var sessions = new FakeSessionRepository();
        await using var application = CreateApplication(plan, sessions);
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/navigation/sessions",
            new StartNavigationSessionRequest(plan.Id, plan.Alternatives[0].Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<StartNavigationSessionResponse>();
        Assert.NotNull(body);
        Assert.Equal("raw-token", body.NavigationToken);
        Assert.Equal("token-hash", sessions.Added?.Hash);
        Assert.Equal($"/v1/navigation/sessions/{body.SessionId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task StartSession_MissingPlan_ReturnsNotFound()
    {
        await using var application = CreateApplication(null, new FakeSessionRepository());
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/navigation/sessions",
            new StartNavigationSessionRequest(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateApplication(RoutePlan? plan, FakeSessionRepository sessions) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Pagination:SigningKey", "integration-test-signing-key");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRoutePlanRepository>();
                services.AddSingleton<IRoutePlanRepository>(new FakePlanRepository(plan));
                services.RemoveAll<INavigationSessionRepository>();
                services.AddSingleton<INavigationSessionRepository>(sessions);
                services.RemoveAll<INavigationTokenService>();
                services.AddSingleton<INavigationTokenService>(new FakeTokenService());
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            });
        });

    private static RoutePlan Plan()
    {
        var request = new RoutePlanRequest(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50m, 19m)),
            new RouteEndpoint.Coordinate(new GeoCoordinate(51m, 20m)), [TravelMode.Walk],
            new Dictionary<Code, ConstraintLevel>(), [new Code("fastest")], "pl-PL", new Dictionary<Code, string>());
        var route = new RouteAlternative(
            Guid.NewGuid(), new Code("fastest"),
            new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(51m, 20m)]),
            [new RouteLeg(Guid.NewGuid(), 0,
                new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50m, 19m), new GeoCoordinate(51m, 20m)]),
                TimeSpan.FromMinutes(1), 10m, [])], TimeSpan.FromMinutes(1), 10m, [],
            new AccessibilitySummary([], [], []), [],
            new ConfidenceAssessment(ConfidenceState.Unverified, 0, 0, Now), new Dictionary<Code, string>(),
            Now, Now.AddMinutes(15));
        return new RoutePlan(Guid.NewGuid(), null, request, [route], Now, Now.AddMinutes(15));
    }

    private sealed class FakePlanRepository(RoutePlan? plan) : IRoutePlanRepository
    {
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(plan?.Id == id ? plan : null);
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
