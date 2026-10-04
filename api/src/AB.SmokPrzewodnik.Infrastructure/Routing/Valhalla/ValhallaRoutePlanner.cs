using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Routing;

namespace AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;

internal sealed class ValhallaRoutePlanner(
    HttpClient httpClient,
    ValhallaOptions options,
    TimeProvider timeProvider) : IRoutePlanner
{
    public async Task<IReadOnlyList<RouteAlternative>> PlanAsync(
        RoutePlanRequest request,
        CancellationToken cancellationToken)
    {
        var alternatives = new List<RouteAlternative>();
        RoutePlanningException? lastFailure = null;

        foreach (var plannedRequest in ValhallaRequestMapper.Map(request, options.Alternates))
        {
            try
            {
                alternatives.AddRange(await PlanAsync(plannedRequest, cancellationToken));
            }
            catch (RoutePlanningException exception)
            {
                lastFailure = exception;
            }
        }

        return alternatives.Count > 0
            ? alternatives
            : throw lastFailure ?? new RoutePlanningException(
                RoutePlanningFailureKind.Unavailable,
                "The routing provider returned no alternatives.");
    }

    private async Task<IReadOnlyList<RouteAlternative>> PlanAsync(
        ValhallaPlannedRequest plannedRequest,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(
                    "route",
                    plannedRequest.Request,
                    ValhallaJson.Options,
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return await MapResponseAsync(response, plannedRequest, cancellationToken);
                }

                if ((int)response.StatusCode < 500)
                {
                    throw new RoutePlanningException(
                        RoutePlanningFailureKind.InvalidRequest,
                        $"Valhalla rejected the route request with status {(int)response.StatusCode}.");
                }

                if (attempt == 2)
                {
                    throw Unavailable(response.StatusCode);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (RoutePlanningException)
            {
                throw;
            }
            catch (HttpRequestException exception) when (attempt == 2)
            {
                throw new RoutePlanningException(
                    RoutePlanningFailureKind.Unavailable,
                    "Valhalla is unavailable.",
                    exception);
            }
            catch (HttpRequestException)
            {
                // Retry the single transient provider failure.
            }
        }

        throw new InvalidOperationException("The Valhalla retry loop exited unexpectedly.");
    }

    private async Task<IReadOnlyList<RouteAlternative>> MapResponseAsync(
        HttpResponseMessage response,
        ValhallaPlannedRequest plannedRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ValhallaOsrmResponse>(
                ValhallaJson.Options,
                cancellationToken);
            if (payload is null)
            {
                throw new JsonException("Valhalla returned an empty response.");
            }

            var createdAt = timeProvider.GetUtcNow();
            return ValhallaResponseMapper.Map(payload, plannedRequest, createdAt, createdAt.AddMinutes(15));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException or ArgumentException)
        {
            throw new RoutePlanningException(
                RoutePlanningFailureKind.InvalidResponse,
                "Valhalla returned an invalid route response.",
                exception);
        }
    }

    private static RoutePlanningException Unavailable(HttpStatusCode statusCode) =>
        new(
            RoutePlanningFailureKind.Unavailable,
            $"Valhalla is unavailable (status {(int)statusCode}).");
}
