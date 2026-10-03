using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class AccessibilityFact : Entity<Guid>
{
    private AccessibilityFact()
    {
        AttributeCode = default;
        TargetKind = string.Empty;
        ValueKind = string.Empty;
        EvidenceKind = string.Empty;
    }

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
        SetTarget(target ?? throw new ArgumentNullException(nameof(target)));
        AttributeCode = attributeCode;
        SetValue(value ?? throw new ArgumentNullException(nameof(value)));
        SetEvidence(evidence ?? throw new ArgumentNullException(nameof(evidence)));
        ObservedAt = observedAt;
        ValidFrom = validFrom;
        ValidUntil = validUntil;
        ConfidenceWeight = Guard.InRange(confidenceWeight, 0, 1, nameof(confidenceWeight));
    }

    public AccessibilityFactTarget Target => TargetKind switch
    {
        "spatial_entity" when SpatialEntityId is { } id => new AccessibilityFactTarget.SpatialEntity(id),
        "graph_element" => new AccessibilityFactTarget.GraphElement(new GraphReference(
            GraphRoutingProvider!, GraphVersion!, GraphElementType!.Value, GraphExternalElementId!)),
        _ => throw new InvalidOperationException("The accessibility fact target is incomplete.")
    };
    internal string TargetKind { get; private set; } = string.Empty;
    public Guid? SpatialEntityId { get; private set; }
    public SpatialEntity? SpatialEntity { get; private set; }
    internal string? GraphRoutingProvider { get; private set; }
    internal string? GraphVersion { get; private set; }
    internal GraphElementType? GraphElementType { get; private set; }
    internal string? GraphExternalElementId { get; private set; }
    public Code AttributeCode { get; private set; }
    public AccessibilityValue Value => ValueKind switch
    {
        "boolean" when BooleanValue is { } value => new AccessibilityValue.Boolean(value),
        "number" when NumberValue is { } value => new AccessibilityValue.Number(value, NumberUnitCode),
        "code" => new AccessibilityValue.Code(new Code(CodeValue!)),
        "text" => new AccessibilityValue.Text(TextValue!),
        _ => throw new InvalidOperationException("The accessibility fact value is incomplete.")
    };
    internal string ValueKind { get; private set; } = string.Empty;
    internal bool? BooleanValue { get; private set; }
    internal decimal? NumberValue { get; private set; }
    internal string? NumberUnitCode { get; private set; }
    internal string? CodeValue { get; private set; }
    internal string? TextValue { get; private set; }
    public EvidenceReference Evidence => EvidenceKind switch
    {
        "source_assertion" when SourceAssertionId is { } id => new EvidenceReference.SourceAssertion(id),
        "observation" when ObservationId is { } id => new EvidenceReference.Observation(id),
        _ => throw new InvalidOperationException("The accessibility fact evidence is incomplete.")
    };
    internal string EvidenceKind { get; private set; } = string.Empty;
    public Guid? SourceAssertionId { get; private set; }
    public SourceAssertion? SourceAssertion { get; private set; }
    internal Guid? ObservationId { get; private set; }
    public DateTimeOffset ObservedAt { get; private set; }
    public DateTimeOffset? ValidFrom { get; private set; }
    public DateTimeOffset? ValidUntil { get; private set; }
    public decimal ConfidenceWeight { get; private set; }

    private void SetTarget(AccessibilityFactTarget target)
    {
        switch (target)
        {
            case AccessibilityFactTarget.SpatialEntity spatial:
                TargetKind = "spatial_entity";
                SpatialEntityId = spatial.EntityId;
                break;
            case AccessibilityFactTarget.GraphElement graph:
                TargetKind = "graph_element";
                GraphRoutingProvider = graph.Reference.RoutingProvider;
                GraphVersion = graph.Reference.GraphVersion;
                GraphElementType = graph.Reference.ElementType;
                GraphExternalElementId = graph.Reference.ExternalElementId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target));
        }
    }

    private void SetValue(AccessibilityValue value)
    {
        switch (value)
        {
            case AccessibilityValue.Boolean boolean:
                ValueKind = "boolean";
                BooleanValue = boolean.Value;
                break;
            case AccessibilityValue.Number number:
                ValueKind = "number";
                NumberValue = number.Value;
                NumberUnitCode = number.UnitCode;
                break;
            case AccessibilityValue.Code code:
                ValueKind = "code";
                CodeValue = code.Value.Value;
                break;
            case AccessibilityValue.Text text:
                ValueKind = "text";
                TextValue = text.Value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    private void SetEvidence(EvidenceReference evidence)
    {
        switch (evidence)
        {
            case EvidenceReference.SourceAssertion assertion:
                EvidenceKind = "source_assertion";
                SourceAssertionId = assertion.AssertionId;
                break;
            case EvidenceReference.Observation observation:
                EvidenceKind = "observation";
                ObservationId = observation.ObservationId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(evidence));
        }
    }
}
