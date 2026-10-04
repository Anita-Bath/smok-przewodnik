using System.Net;
using System.Net.Http.Json;
using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Application.Routing.Dtos;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests.Routing;

public sealed class RoutesEndpointTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task PlanRoute_AnonymousRequest_ReturnsAndPersistsPlan()
    {
        var plans = new FakeRoutePlanRepository();
        await using var application = CreateApplication(new FakePlanner([Alternative()]), plans);
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/routes/plan", Request());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<RoutePlanResponse>();
        Assert.NotNull(body);
        Assert.Single(body.Alternatives);
        Assert.NotNull(plans.Added);
        Assert.Null(plans.Added.AccountId);
    }

    [Fact]
    public async Task PlanRoute_WhenNoCandidateSatisfiesHardConstraint_ReturnsConflict()
    {
        var plans = new FakeRoutePlanRepository();
        await using var application = CreateApplication(new FakePlanner([Alternative([new Code("stairs")])]), plans);
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/routes/plan", Request(
            new Dictionary<string, ConstraintLevel> { ["stairs"] = ConstraintLevel.MustAvoid }));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null(plans.Added);
    }

    [Theory]
    [InlineData(RoutePlanningFailureKind.InvalidResponse, HttpStatusCode.BadGateway)]
    [InlineData(RoutePlanningFailureKind.Unavailable, HttpStatusCode.ServiceUnavailable)]
    public async Task PlanRoute_MapsProviderFailures(RoutePlanningFailureKind kind, HttpStatusCode status)
    {
        await using var application = CreateApplication(new FakePlanner(kind), new FakeRoutePlanRepository());
        using var client = application.CreateClient();

        var response = await client.PostAsJsonAsync("/v1/routes/plan", Request());

        Assert.Equal(status, response.StatusCode);
    }

    private static PlanRouteRequestDto Request(IReadOnlyDictionary<string, ConstraintLevel>? constraints = null) => new(
        new RouteEndpointDto(50.06m, 19.93m, null),
        new RouteEndpointDto(50.08m, 19.96m, null),
        [TravelMode.Walk],
        constraints ?? new Dictionary<string, ConstraintLevel>(),
        ["fastest"],
        "pl-PL",
        new Dictionary<string, string>());

    private static RouteAlternative Alternative(IEnumerable<Code>? unknowns = null) => new(
        Guid.NewGuid(), new Code("fastest"),
        new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50.06m, 19.93m), new GeoCoordinate(50.08m, 19.96m)]),
        [new RouteLeg(Guid.NewGuid(), 0,
            new SpatialGeometry(GeometryKind.Line, [new GeoCoordinate(50.06m, 19.93m), new GeoCoordinate(50.08m, 19.96m)]),
            TimeSpan.FromMinutes(5), 500m, [])],
        TimeSpan.FromMinutes(5), 500m, [], new AccessibilitySummary([], unknowns ?? [], []), [],
        new ConfidenceAssessment(ConfidenceState.Unverified, 0m, 0, Now),
        new Dictionary<Code, string> { [new Code("valhalla")] = "3.x" }, Now, Now.AddMinutes(15));

    private static WebApplicationFactory<Program> CreateApplication(
        IRoutePlanner planner,
        IRoutePlanRepository plans) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Pagination:SigningKey", "integration-test-signing-key");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IRoutePlanner>();
                services.AddSingleton(planner);
                services.RemoveAll<IRoutePlanRepository>();
                services.AddSingleton(plans);
                services.RemoveAll<ISpatialEntityRepository>();
                services.AddSingleton<ISpatialEntityRepository>(new EmptySpatialRepository());
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            });
        });

    private sealed class FakePlanner : IRoutePlanner
    {
        private readonly IReadOnlyList<RouteAlternative>? _alternatives;
        private readonly RoutePlanningFailureKind? _failure;
        public FakePlanner(IReadOnlyList<RouteAlternative> alternatives) => _alternatives = alternatives;
        public FakePlanner(RoutePlanningFailureKind failure) => _failure = failure;
        public Task<IReadOnlyList<RouteAlternative>> PlanAsync(RoutePlanRequest request, CancellationToken cancellationToken) =>
            _failure.HasValue
                ? Task.FromException<IReadOnlyList<RouteAlternative>>(new RoutePlanningException(_failure.Value, "provider failure"))
                : Task.FromResult(_alternatives!);
    }

    private sealed class FakeRoutePlanRepository : IRoutePlanRepository
    {
        public RoutePlan? Added { get; private set; }
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RoutePlan?>(null);
        public Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken) { Added = routePlan; return Task.CompletedTask; }
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) => Task.FromResult(0);
    }

    private sealed class EmptySpatialRepository : ISpatialEntityRepository
    {
        public Task<SpatialEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<SpatialEntity?>(null);
        public Task<CursorPage<SpatialEntity>> FindAsync(SpatialEntityCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) =>
            Task.FromResult(new CursorPage<SpatialEntity>([], null));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
