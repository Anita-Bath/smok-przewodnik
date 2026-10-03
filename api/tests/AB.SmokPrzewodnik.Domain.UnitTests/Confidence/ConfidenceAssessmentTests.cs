using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Confidence;

public sealed class ConfidenceAssessmentTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Constructor_RejectsScoreOutsideUnitInterval(decimal score)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConfidenceAssessment(ConfidenceState.Supported, score, 1, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_RejectsNegativeEvidenceCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ConfidenceAssessment(ConfidenceState.Unverified, 0, -1, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Constructor_PreservesValidValues()
    {
        var evaluatedAt = DateTimeOffset.Parse("2026-10-03T12:00:00+02:00");

        var assessment = new ConfidenceAssessment(ConfidenceState.Supported, 0.75m, 4, evaluatedAt);

        Assert.Equal(ConfidenceState.Supported, assessment.State);
        Assert.Equal(0.75m, assessment.Score);
        Assert.Equal(4, assessment.EvidenceCount);
        Assert.Equal(evaluatedAt, assessment.EvaluatedAt);
    }
}
