namespace AB.SmokPrzewodnik.Application.Routing;

public sealed class RouteEndpointNotFoundException(Guid entityId)
    : Exception($"Spatial entity '{entityId}' was not found.")
{
    public Guid EntityId { get; } = entityId;
}

public sealed class NoViableRouteException()
    : Exception("No route satisfies the requested hard constraints.");
