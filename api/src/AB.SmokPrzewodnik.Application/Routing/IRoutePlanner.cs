using AB.SmokPrzewodnik.Domain.Routing;

namespace AB.SmokPrzewodnik.Application.Routing;

public interface IRoutePlanner
{
    Task<IReadOnlyList<RouteAlternative>> PlanAsync(
        RoutePlanRequest request,
        CancellationToken cancellationToken);
}
