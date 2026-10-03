using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed class SourceAssertion(Guid id) : Entity<Guid>(id)
{
    public Guid SourceId { get; }
    public string ExternalId { get; }

    public TaxonomyCode AssertionType { get; }
    public SourcePayload Payload { get; }

    public DateTimeOffset RetrievedAt { get; }
    public DateTimeOffset? ValidFrom { get; }
    public DateTimeOffset? ValidUntil { get; }

    public string TransformVersion { get; }
}
