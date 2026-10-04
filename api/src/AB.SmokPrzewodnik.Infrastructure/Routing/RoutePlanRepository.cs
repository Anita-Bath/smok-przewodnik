using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Routing;
using Microsoft.EntityFrameworkCore;

namespace AB.SmokPrzewodnik.Infrastructure.Routing;

internal sealed class RoutePlanRepository(Database.DbContext dbContext) : IRoutePlanRepository
{
    public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.RoutePlans
            .AsNoTracking()
            .SingleOrDefaultAsync(routePlan => routePlan.Id == id, cancellationToken);

    public async Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(routePlan);
        dbContext.RoutePlans.Add(routePlan);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) =>
        dbContext.RoutePlans
            .Where(routePlan => routePlan.ExpiresAt <= now)
            .ExecuteDeleteAsync(cancellationToken);
}
