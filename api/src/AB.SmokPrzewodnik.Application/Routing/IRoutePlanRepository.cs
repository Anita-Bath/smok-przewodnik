using AB.SmokPrzewodnik.Domain.Routing;

namespace AB.SmokPrzewodnik.Application.Routing;

public interface IRoutePlanRepository
{
    Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken);
}
