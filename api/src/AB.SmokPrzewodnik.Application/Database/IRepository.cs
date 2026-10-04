using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Application.Database;

public interface IRepository<TEntity, TId, in TCriteria>
      where TEntity : Entity<TId>
      where TId : notnull
{
    Task<TEntity?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken);

    Task<CursorPage<TEntity>> FindAsync(
        TCriteria criteria,
        CursorPageRequest page,
        CancellationToken cancellationToken);
}
