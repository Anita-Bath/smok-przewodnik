using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed record FeedItem
{
    private readonly List<FeedItemEntityLink> _entityLinks = [];

    private FeedItem()
    {
        ContentType = default;
        LocalizedContent = null!;
        Provenance = default;
        AudienceTags = FrozenSet<Code>.Empty;
    }

    public FeedItem(
        Guid id,
        Guid cityId,
        Code contentType,
        Guid sourceId,
        IEnumerable<Guid> linkedEntityIds,
        LocalizedContent localizedContent,
        DateTimeOffset publishedAt,
        DateTimeOffset? validUntil,
        IEnumerable<Code> audienceTags,
        Code provenance)
    {
        Id = RequireId(id, nameof(id));
        CityId = RequireId(cityId, nameof(cityId));
        SourceId = RequireId(sourceId, nameof(sourceId));
        ContentType = contentType;
        LocalizedContent = localizedContent ?? throw new ArgumentNullException(nameof(localizedContent));
        Provenance = provenance;

        ArgumentNullException.ThrowIfNull(linkedEntityIds);
        var entities = linkedEntityIds.Distinct().Select(value => RequireId(value, nameof(linkedEntityIds))).ToList();
        if (entities.Count == 0)
        {
            throw new ArgumentException("At least one linked entity is required.", nameof(linkedEntityIds));
        }

        _entityLinks.AddRange(entities.Select(entityId => new FeedItemEntityLink(Id, entityId)));

        ArgumentNullException.ThrowIfNull(audienceTags);
        AudienceTags = audienceTags.ToFrozenSet();
        if (AudienceTags.Count == 0)
        {
            throw new ArgumentException("At least one audience tag is required.", nameof(audienceTags));
        }

        if (validUntil < publishedAt)
        {
            throw new ArgumentException("Validity cannot end before publication.", nameof(validUntil));
        }

        PublishedAt = publishedAt;
        ValidUntil = validUntil;
    }

    public Guid Id { get; private set; }

    public Guid CityId { get; private set; }

    public City City { get; private set; } = null!;

    public Code ContentType { get; private set; }

    public Guid SourceId { get; private set; }

    public DataSource Source { get; private set; } = null!;

    public IReadOnlyList<Guid> LinkedEntityIds => _entityLinks.Select(link => link.EntityId).ToArray();

    public IReadOnlyCollection<SpatialEntity> LinkedEntities => _entityLinks
        .Where(link => link.Entity is not null)
        .Select(link => link.Entity)
        .ToArray();

    internal IReadOnlyCollection<FeedItemEntityLink> EntityLinks => _entityLinks;

    public LocalizedContent LocalizedContent { get; private set; }

    public DateTimeOffset PublishedAt { get; private set; }

    public DateTimeOffset? ValidUntil { get; private set; }

    public IReadOnlySet<Code> AudienceTags { get; private set; }

    public Code Provenance { get; private set; }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }
}
