using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed class AccessibilityFact(Guid id) : Entity<Guid>(id)
{
    public AccessibilityFactTarget Target { get; private set; }
    public TaxonomyCode AttributeCode { get; private set; }
    public AccessibilityValue Value { get; private set; }
    public EvidenceReference Evidence { get; private set; }

    public DateTimeOffset ObservedAt { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidUntil { get; private set; }

    public decimal ConfidenceWeight { get; private set; }
}
