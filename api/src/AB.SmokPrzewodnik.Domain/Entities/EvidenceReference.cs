namespace AB.SmokPrzewodnik.Domain.Entities;

public abstract record EvidenceReference
{
    public sealed record SourceAssertion(Guid AssertionId)
        : EvidenceReference;

    public sealed record Observation(Guid ObservationId)
        : EvidenceReference;
}
