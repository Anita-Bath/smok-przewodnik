using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Application.Navigation;
using Microsoft.AspNetCore.Diagnostics;

namespace AB.SmokPrzewodnik.Api.Errors;

internal sealed class ApplicationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is InvalidCursorException invalidCursor)
        {
            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status400BadRequest,
                "Invalid pagination cursor.",
                invalidCursor.Message,
                invalidCursor.Code);
            return true;
        }

        var problem = exception switch
        {
            RouteEndpointNotFoundException => (StatusCodes.Status404NotFound, "Route endpoint not found.", "route_endpoint_not_found"),
            NoViableRouteException => (StatusCodes.Status409Conflict, "No viable route.", "no_viable_route"),
            RoutePlanNotFoundException or RouteAlternativeNotFoundException =>
                (StatusCodes.Status404NotFound, "Route plan selection not found.", "route_plan_selection_not_found"),
            RoutePlanExpiredException =>
                (StatusCodes.Status409Conflict, "Route plan expired.", "route_plan_expired"),
            RoutePlanAccessDeniedException =>
                (StatusCodes.Status403Forbidden, "Route plan access denied.", "route_plan_access_denied"),
            NavigationSessionNotFoundException =>
                (StatusCodes.Status404NotFound, "Navigation session not found.", "navigation_session_not_found"),
            NavigationSessionExpiredException =>
                (StatusCodes.Status409Conflict, "Navigation session expired.", "navigation_session_expired"),
            NavigationConcurrencyException =>
                (StatusCodes.Status409Conflict, "Navigation session changed concurrently.", "navigation_concurrency_conflict"),
            RoutePlanningException { Kind: RoutePlanningFailureKind.InvalidRequest } =>
                (StatusCodes.Status400BadRequest, "Invalid route request.", "invalid_route_request"),
            RoutePlanningException { Kind: RoutePlanningFailureKind.InvalidResponse } =>
                (StatusCodes.Status502BadGateway, "Invalid routing provider response.", "routing_provider_invalid_response"),
            RoutePlanningException { Kind: RoutePlanningFailureKind.Unavailable } =>
                (StatusCodes.Status503ServiceUnavailable, "Routing provider unavailable.", "routing_provider_unavailable"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", "invalid_request"),
            _ => ((int Status, string Title, string Code)?)null
        };

        if (problem is null)
        {
            return false;
        }

        await WriteProblemAsync(httpContext, problem.Value.Status, problem.Value.Title, exception.Message, problem.Value.Code);

        return true;
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        string code) =>
        Results.Problem(
            statusCode: status,
            title: title,
            detail: detail,
            extensions: new Dictionary<string, object?> { ["code"] = code })
        .ExecuteAsync(context);
}
