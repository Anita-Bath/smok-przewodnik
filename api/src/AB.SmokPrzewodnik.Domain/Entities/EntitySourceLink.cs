using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed class EntitySourceLink(Guid id) : AuditableEntity<Guid>(id)
{
    public Guid EntityId { get; set; }
    public Guid SourceId { get; set; }

    public string ExternalId { get; private set; }
    public DateTimeOffset RetrievedAt { get; private set; }

    public SourceLicenseMetadata License { get; private set; }
    public string TransformVersion { get; private set; }
}
