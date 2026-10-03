using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class SourceAssertion : Entity<Guid>
{
    private readonly List<AccessibilityFact> _accessibilityFacts = [];
    private readonly ReadOnlyCollection<AccessibilityFact> _accessibilityFactView;

    public SourceAssertion(
        Guid id,
        Guid sourceId,
        string externalId,
        Code assertionType,
        SourcePayload payload,
        DateTimeOffset retrievedAt,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil,
        string transformVersion)
        : base(id)
    {
        if (sourceId == Guid.Empty)
        {
            throw new ArgumentException("Source ID cannot be empty.", nameof(sourceId));
        }

        Guard.ValidTimeRange(validFrom, validUntil);
        SourceId = sourceId;
        ExternalId = Guard.NotBlank(externalId, nameof(externalId));
        AssertionType = assertionType;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
        RetrievedAt = retrievedAt;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        TransformVersion = Guard.NotBlank(transformVersion, nameof(transformVersion));
        _accessibilityFactView = _accessibilityFacts.AsReadOnly();
    }

    public Guid SourceId { get; }
    public DataSource Source { get; private set; } = null!;
    public string ExternalId { get; }
    public Code AssertionType { get; }
    public SourcePayload Payload { get; }
    public DateTimeOffset RetrievedAt { get; }
    public DateTimeOffset? ValidFrom { get; }
    public DateTimeOffset? ValidUntil { get; }
    public string TransformVersion { get; }
    public IReadOnlyCollection<AccessibilityFact> AccessibilityFacts => _accessibilityFactView;
}
