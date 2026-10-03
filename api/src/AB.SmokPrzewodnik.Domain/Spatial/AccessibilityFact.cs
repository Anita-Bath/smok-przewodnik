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
        AttributeCode = attributeCode;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        ObservedAt = observedAt;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        ConfidenceWeight = Guard.InRange(confidenceWeight, 0, 1, nameof(confidenceWeight));
    }

    public AccessibilityFactTarget Target { get; }
    public Code AttributeCode { get; }
    public AccessibilityValue Value { get; }
    public EvidenceReference Evidence { get; }
    public DateTimeOffset ObservedAt { get; }
    public DateTimeOffset? ValidFrom { get; }
    public DateTimeOffset? ValidUntil { get; }
    public decimal ConfidenceWeight { get; }
}
