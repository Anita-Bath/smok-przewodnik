using System.Net;
using System.Text;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Routing;

public sealed class ValhallaRoutePlannerTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T10:00:00Z");

    [Fact]
    public async Task PlanAsync_SerializesRequestAndMapsResponse()
    {
        string? body = null;
        var handler = new StubHttpMessageHandler(async (request, _, cancellationToken) =>
        {
            body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return JsonResponse(ValhallaTestData.ResponseJson());
        });
        var planner = CreatePlanner(handler);

        var alternatives = await planner.PlanAsync(CreateRequest([TravelMode.Walk]), CancellationToken.None);

        Assert.Single(alternatives);
        Assert.Contains("\"costing\":\"pedestrian\"", body, StringComparison.Ordinal);
        Assert.Contains("\"format\":\"osrm\"", body, StringComparison.Ordinal);
        Assert.Contains("\"shape_format\":\"geojson\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PlanAsync_RetriesTransientServerFailureOnce()
    {
        var handler = new StubHttpMessageHandler((_, attempt, _) => Task.FromResult(
            attempt == 1
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : JsonResponse(ValhallaTestData.ResponseJson())));
        var planner = CreatePlanner(handler);

        var alternatives = await planner.PlanAsync(CreateRequest([TravelMode.Walk]), CancellationToken.None);

        Assert.Single(alternatives);
        Assert.Equal(2, handler.Attempts);
    }

    [Fact]
    public async Task PlanAsync_ReturnsSuccessfulModeWhenAnotherModeFails()
    {
        var handler = new StubHttpMessageHandler(async (request, _, cancellationToken) =>
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return body.Contains("\"costing\":\"pedestrian\"", StringComparison.Ordinal)
                ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                : JsonResponse(ValhallaTestData.ResponseJson());
        });
        var planner = CreatePlanner(handler);

        var alternatives = await planner.PlanAsync(
            CreateRequest([TravelMode.Walk, TravelMode.Bicycle]),
            CancellationToken.None);

        Assert.Single(alternatives);
    }

    [Fact]
    public async Task PlanAsync_InvalidJson_ThrowsInvalidResponse()
    {
        var planner = CreatePlanner(new StubHttpMessageHandler((_, _, _) =>
            Task.FromResult(JsonResponse("not-json"))));

        var exception = await Assert.ThrowsAsync<RoutePlanningException>(() =>
            planner.PlanAsync(CreateRequest([TravelMode.Walk]), CancellationToken.None));

        Assert.Equal(RoutePlanningFailureKind.InvalidResponse, exception.Kind);
    }

    [Fact]
    public async Task PlanAsync_ClientError_IsNotRetried()
    {
        var handler = new StubHttpMessageHandler((_, _, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.BadRequest)));
        var planner = CreatePlanner(handler);

        var exception = await Assert.ThrowsAsync<RoutePlanningException>(() =>
            planner.PlanAsync(CreateRequest([TravelMode.Walk]), CancellationToken.None));

        Assert.Equal(RoutePlanningFailureKind.InvalidRequest, exception.Kind);
        Assert.Equal(1, handler.Attempts);
    }

    [Fact]
    public async Task PlanAsync_TotalServerFailure_ThrowsUnavailableAfterRetry()
    {
        var handler = new StubHttpMessageHandler((_, _, _) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.InternalServerError)));
        var planner = CreatePlanner(handler);

        var exception = await Assert.ThrowsAsync<RoutePlanningException>(() =>
            planner.PlanAsync(CreateRequest([TravelMode.Walk]), CancellationToken.None));

        Assert.Equal(RoutePlanningFailureKind.Unavailable, exception.Kind);
        Assert.Equal(2, handler.Attempts);
    }

    [Fact]
    public async Task PlanAsync_CallerCancellation_IsPropagated()
    {
        var handler = new StubHttpMessageHandler(async (_, _, cancellationToken) =>
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return JsonResponse(ValhallaTestData.ResponseJson());
        });
        var planner = CreatePlanner(handler);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            planner.PlanAsync(CreateRequest([TravelMode.Walk]), cancellation.Token));
        Assert.Equal(1, handler.Attempts);
    }

    private static ValhallaRoutePlanner CreatePlanner(HttpMessageHandler handler) =>
        new(
            new HttpClient(handler) { BaseAddress = new Uri("http://valhalla.test/") },
            new ValhallaOptions
            {
                BaseUrl = new Uri("http://valhalla.test/"),
                TimeoutSeconds = 5,
                Alternates = 2
            },
            new FixedTimeProvider(Now));

    private static RoutePlanRequest CreateRequest(IReadOnlyCollection<TravelMode> modes) =>
        new(
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.0617m, 19.9373m)),
            new RouteEndpoint.Coordinate(new GeoCoordinate(50.067m, 19.945m)),
            modes,
            new Dictionary<Code, ConstraintLevel>(),
            [new Code("fastest")],
            "pl-PL",
            new Dictionary<Code, string>());

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, int, CancellationToken, Task<HttpResponseMessage>> responseFactory)
        : HttpMessageHandler
    {
        public int Attempts { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Attempts++;
            return responseFactory(request, Attempts, cancellationToken);
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}

internal static class ValhallaTestData
{
    public static ValhallaPlannedRequest PlannedRequest(
        TravelMode mode,
        string profile,
        IReadOnlySet<string> hardConstraints) =>
        new(
            mode,
            profile,
            new ValhallaRouteRequest([], "pedestrian", new Dictionary<string, IReadOnlyDictionary<string, System.Text.Json.JsonElement>>(), "kilometers", "pl-PL", 1),
            hardConstraints);

    public static ValhallaOsrmResponse Response(int routeCount = 1) =>
        System.Text.Json.JsonSerializer.Deserialize<ValhallaOsrmResponse>(ResponseJson(routeCount), ValhallaJson.Options)!;

    public static string ResponseJson(int routeCount = 1)
    {
        const string route = """
            {
              "geometry": { "type": "LineString", "coordinates": [[19.9373,50.0617],[19.94,50.063],[19.945,50.067]] },
              "distance": 620.0,
              "duration": 480.0,
              "legs": [{
                "distance": 620.0,
                "duration": 480.0,
                "steps": [
                  { "distance": 300.0, "duration": 220.0, "geometry": { "type": "LineString", "coordinates": [[19.9373,50.0617],[19.94,50.063]] }, "maneuver": { "type": "depart", "modifier": "straight", "location": [19.9373,50.0617] } },
                  { "distance": 320.0, "duration": 260.0, "geometry": { "type": "LineString", "coordinates": [[19.94,50.063],[19.945,50.067]] }, "maneuver": { "type": "turn", "modifier": "right", "location": [19.94,50.063] } }
                ]
              }]
            }
            """;

        return $$"""
            { "code": "Ok", "routes": [{{string.Join(',', Enumerable.Repeat(route, routeCount))}}] }
            """;
    }
}
