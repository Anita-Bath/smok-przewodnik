namespace AB.SmokPrzewodnik.Application.Navigation;

public sealed class RoutePlanNotFoundException(Guid routePlanId)
    : Exception($"Route plan '{routePlanId}' was not found.");

public sealed class RouteAlternativeNotFoundException(Guid alternativeId)
    : Exception($"Route alternative '{alternativeId}' was not found in the route plan.");

public sealed class RoutePlanExpiredException(Guid routePlanId)
    : Exception($"Route plan '{routePlanId}' has expired.");

public sealed class RoutePlanAccessDeniedException(Guid routePlanId)
    : Exception($"Route plan '{routePlanId}' belongs to another account.");

public sealed class NavigationSessionNotFoundException(Guid sessionId)
    : Exception($"Navigation session '{sessionId}' was not found.");

public sealed class NavigationSessionExpiredException(Guid sessionId)
    : Exception($"Navigation session '{sessionId}' has expired.");

public sealed class NavigationConcurrencyException(Exception? innerException = null)
    : Exception("The navigation session was modified concurrently.", innerException);
