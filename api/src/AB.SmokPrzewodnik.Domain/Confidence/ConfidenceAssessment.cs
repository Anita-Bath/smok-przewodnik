using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Confidence;

public sealed record ConfidenceAssessment
{
    private ConfidenceAssessment()
    {
    }

    public ConfidenceAssessment(
        ConfidenceState state,
        decimal score,
        int evidenceCount,
        DateTimeOffset evaluatedAt)
    {
        if (score is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(score));
        }

        if (evidenceCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(evidenceCount));
        }

        State = state;
        Score = score;
        EvidenceCount = evidenceCount;
        EvaluatedAt = evaluatedAt;
    }

    public ConfidenceState State { get; private set; }

    public decimal Score { get; private set; }

    public int EvidenceCount { get; private set; }

    public DateTimeOffset EvaluatedAt { get; private set; }
}
