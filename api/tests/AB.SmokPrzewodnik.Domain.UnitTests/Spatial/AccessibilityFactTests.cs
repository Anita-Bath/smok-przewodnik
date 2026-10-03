using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Spatial;

public sealed class AccessibilityFactTests
{
    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Constructor_RejectsConfidenceOutsideUnitInterval(decimal confidenceWeight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateFact(confidenceWeight));
    }

    [Fact]
    public void Constructor_PreservesEntityTargetAndSourceEvidence()
    {
        var entityId = Guid.NewGuid();
        var assertionId = Guid.NewGuid();

        var fact = new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.SpatialEntity(entityId),
            new Code("step_free_access"),
            new AccessibilityValue.Boolean(true),
            new EvidenceReference.SourceAssertion(assertionId),
            DateTimeOffset.UtcNow,
            null,
            null,
            0.8m);

        Assert.Equal(new AccessibilityFactTarget.SpatialEntity(entityId), fact.Target);
        Assert.Equal(new EvidenceReference.SourceAssertion(assertionId), fact.Evidence);
        Assert.Equal(new AccessibilityValue.Boolean(true), fact.Value);
    }

    [Fact]
    public void Constructor_PreservesGraphTargetAndObservationEvidence()
    {
        var graph = new GraphReference("valhalla", "v1", GraphElementType.Edge, "42");
        var observationId = Guid.NewGuid();

        var fact = new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.GraphElement(graph),
            new Code("surface"),
            new AccessibilityValue.Code(new Code("cobblestone")),
            new EvidenceReference.Observation(observationId),
            DateTimeOffset.UtcNow,
            null,
            null,
            0.5m);

        Assert.Equal(new AccessibilityFactTarget.GraphElement(graph), fact.Target);
        Assert.Equal(new AccessibilityValue.Code(new Code("cobblestone")), fact.Value);
    }

    [Fact]
    public void Constructor_RejectsReversedValidityRange()
    {
        Assert.Throws<ArgumentException>(() => new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.SpatialEntity(Guid.NewGuid()),
            new Code("door_width"),
            new AccessibilityValue.Number(80, "cm"),
            new EvidenceReference.Observation(Guid.NewGuid()),
            DateTimeOffset.UtcNow,
            DateTimeOffset.Parse("2026-10-04T00:00:00Z"),
            DateTimeOffset.Parse("2026-10-03T00:00:00Z"),
            0.5m));
    }

    private static AccessibilityFact CreateFact(decimal confidenceWeight) => new(
        Guid.NewGuid(),
        new AccessibilityFactTarget.SpatialEntity(Guid.NewGuid()),
        new Code("step_free_access"),
        new AccessibilityValue.Boolean(true),
        new EvidenceReference.Observation(Guid.NewGuid()),
        DateTimeOffset.UtcNow,
        null,
        null,
        confidenceWeight);
}
