using AB.SmokPrzewodnik.Application.Database;
using AB.SmokPrzewodnik.Domain.Spatial;

namespace AB.SmokPrzewodnik.Application.Spatials;

public interface ISpatialEntityRepository
    : IRepository<SpatialEntity, Guid, SpatialEntityCriteria>
{
    Task UpdateAsync(SpatialEntity aggregate, CancellationToken cancellationToken);
}
