using System.Collections.Frozen;
using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed record FeedItem
{
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

        LinkedEntityIds = new ReadOnlyCollection<Guid>(entities);

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

    public Guid Id { get; }

    public Guid CityId { get; }

    public Code ContentType { get; }

    public Guid SourceId { get; }

    public IReadOnlyList<Guid> LinkedEntityIds { get; }

    public LocalizedContent LocalizedContent { get; }

    public DateTimeOffset PublishedAt { get; }

    public DateTimeOffset? ValidUntil { get; }

    public IReadOnlySet<Code> AudienceTags { get; }

    public Code Provenance { get; }

    private static Guid RequireId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The identifier cannot be empty.", parameterName);
        }

        return id;
    }
}
