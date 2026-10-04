using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Application.Navigation;

public interface INavigationProgressMatcher
{
    NavigationProgressMatch Match(RouteAlternative route, NavigationProgress progress);
}

public sealed record NavigationProgressMatch(
    GeoCoordinate MatchedPosition,
    decimal RemainingDistanceMetres,
    bool IsOffRoute,
    Maneuver? NextManeuver);
