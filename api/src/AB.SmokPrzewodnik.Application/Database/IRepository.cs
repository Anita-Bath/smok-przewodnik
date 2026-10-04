using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Application.Database;

public interface IRepository<TAggregate, TId, in TCriteria>
      where TAggregate : AggregateRoot<TId>
      where TId : notnull
{
    Task<TAggregate?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken);

    Task<CursorPage<TAggregate>> FindAsync(
        TCriteria criteria,
        CursorPageRequest page,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        TAggregate aggregate,
        CancellationToken cancellationToken);
}
