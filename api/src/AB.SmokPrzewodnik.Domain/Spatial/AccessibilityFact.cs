using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class AccessibilityFact : Entity<Guid>
{
    public AccessibilityFact(
        Guid id,
        AccessibilityFactTarget target,
        Code attributeCode,
        AccessibilityValue value,
        EvidenceReference evidence,
        DateTimeOffset observedAt,
        DateTimeOffset? validFrom,
        DateTimeOffset? validUntil,
        decimal confidenceWeight)
        : base(id)
    {
        Guard.ValidTimeRange(validFrom, validUntil);
        Target = target ?? throw new ArgumentNullException(nameof(target));
        SpatialEntityId = (target as AccessibilityFactTarget.SpatialEntity)?.EntityId;
        AttributeCode = attributeCode;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        SourceAssertionId = (evidence as EvidenceReference.SourceAssertion)?.AssertionId;
        ObservedAt = observedAt;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        ConfidenceWeight = Guard.InRange(confidenceWeight, 0, 1, nameof(confidenceWeight));
    }

    public AccessibilityFactTarget Target { get; }
    public Guid? SpatialEntityId { get; private set; }
    public SpatialEntity? SpatialEntity { get; private set; }
    public Code AttributeCode { get; }
    public AccessibilityValue Value { get; }
    public EvidenceReference Evidence { get; }
    public Guid? SourceAssertionId { get; private set; }
    public SourceAssertion? SourceAssertion { get; private set; }
    public DateTimeOffset ObservedAt { get; }
    public DateTimeOffset? ValidFrom { get; }
    public DateTimeOffset? ValidUntil { get; }
    public decimal ConfidenceWeight { get; }
}
