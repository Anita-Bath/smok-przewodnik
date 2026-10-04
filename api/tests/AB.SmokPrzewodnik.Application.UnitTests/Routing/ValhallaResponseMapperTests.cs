using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Routing;

public sealed class ValhallaResponseMapperTests
{
    private static readonly DateTimeOffset CreatedAt = DateTimeOffset.Parse("2026-10-04T10:00:00Z");

    [Fact]
    public void Map_ConvertsGeometryLegsManeuversAndSummary()
    {
        var planned = ValhallaTestData.PlannedRequest(
            TravelMode.Walk,
            "easiest",
            new HashSet<string> { "stairs" });
        var response = ValhallaTestData.Response();

        var alternative = Assert.Single(ValhallaResponseMapper.Map(
            response,
            planned,
            CreatedAt,
            CreatedAt.AddMinutes(15)));

        Assert.Equal("easiest", alternative.LabelKey.Value);
        Assert.Equal(GeometryKind.Line, alternative.Geometry.Kind);
        Assert.Equal(3, alternative.Geometry.Coordinates.Count);
        Assert.Equal(620m, alternative.DistanceMetres);
        Assert.Equal(TimeSpan.FromSeconds(480), alternative.Duration);
        var leg = Assert.Single(alternative.Legs);
        Assert.Equal(620m, leg.DistanceMetres);
        Assert.Equal(2, leg.Maneuvers.Count);
        Assert.Equal("depart.straight", leg.Maneuvers[0].InstructionKey.Value);
        Assert.Equal("turn.right", leg.Maneuvers[1].InstructionKey.Value);
        Assert.Contains(new Code("stairs"), alternative.AccessibilitySummary.RelevantUnknowns);
        Assert.Equal(ConfidenceState.Unverified, alternative.ConfidenceSummary.State);
    }

    [Fact]
    public void Map_ConvertsEveryReturnedAlternative()
    {
        var response = ValhallaTestData.Response(routeCount: 2);

        var alternatives = ValhallaResponseMapper.Map(
            response,
            ValhallaTestData.PlannedRequest(TravelMode.Bicycle, "fastest", new HashSet<string>()),
            CreatedAt,
            CreatedAt.AddMinutes(15));

        Assert.Equal(2, alternatives.Count);
        Assert.Equal(2, alternatives.Select(alternative => alternative.Id).Distinct().Count());
    }
}
