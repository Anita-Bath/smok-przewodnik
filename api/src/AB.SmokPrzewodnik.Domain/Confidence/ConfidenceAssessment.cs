using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Confidence;

public sealed record ConfidenceAssessment
{
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

    public ConfidenceState State { get; }

    public decimal Score { get; }

    public int EvidenceCount { get; }

    public DateTimeOffset EvaluatedAt { get; }
}
