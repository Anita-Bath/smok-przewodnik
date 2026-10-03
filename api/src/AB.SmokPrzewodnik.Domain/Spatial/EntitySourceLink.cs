using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class EntitySourceLink : AuditableEntity<Guid>
{
    private EntitySourceLink()
    {
        ExternalId = string.Empty;
        License = null!;
        TransformVersion = string.Empty;
    }

    public EntitySourceLink(
        Guid id,
        Guid entityId,
        Guid sourceId,
        string externalId,
        DateTimeOffset retrievedAt,
        SourceLicenseMetadata license,
        string transformVersion,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
        : base(id, createdAt, updatedAt)
    {
        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("Entity ID cannot be empty.", nameof(entityId));
        }

        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException("Source ID cannot be empty.", nameof(sourceId));
        }

        EntityId = entityId;
        SourceId = sourceId;
        ExternalId = Guard.NotBlank(externalId, nameof(externalId));
        RetrievedAt = retrievedAt;
        License = license ?? throw new ArgumentNullException(nameof(license));
        TransformVersion = Guard.NotBlank(transformVersion, nameof(transformVersion));
    }

    public Guid EntityId { get; private set; }

    public SpatialEntity Entity { get; private set; } = null!;

    public Guid SourceId { get; private set; }

    public DataSource Source { get; private set; } = null!;

    public string ExternalId { get; private set; }

    public DateTimeOffset RetrievedAt { get; private set; }

    public SourceLicenseMetadata License { get; private set; }

    public string TransformVersion { get; private set; }
}
