namespace AB.SmokPrzewodnik.Domain.Spatial;

public abstract record EvidenceReference
{
    private EvidenceReference()
    {
    }

    public sealed record SourceAssertion : EvidenceReference
    {
        public SourceAssertion(Guid assertionId)
        {
            if (assertionId == Guid.Empty)
            {
                throw new ArgumentException("Assertion ID cannot be empty.", nameof(assertionId));
            }

            AssertionId = assertionId;
        }

        public Guid AssertionId { get; }
    }

    public sealed record Observation : EvidenceReference
    {
        public Observation(Guid observationId)
        {
            if (observationId == Guid.Empty)
            {
                throw new ArgumentException("Observation ID cannot be empty.", nameof(observationId));
            }

            ObservationId = observationId;
        }

        public Guid ObservationId { get; }
    }
}
