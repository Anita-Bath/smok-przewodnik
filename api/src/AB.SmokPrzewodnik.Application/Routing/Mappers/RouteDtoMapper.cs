using AB.SmokPrzewodnik.Application.Routing.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Application.Routing.Mappers;

public static class RouteDtoMapper
{
    public static RouteAlternativeDto ToDto(RouteAlternative alternative) => new(
        alternative.Id,
        alternative.LabelKey.Value,
        ToGeometry(alternative.Geometry),
        alternative.Legs.Select(leg => new RouteLegDto(
            leg.Id,
            leg.Position,
            ToGeometry(leg.Geometry),
            leg.Duration,
            leg.DistanceMetres,
            leg.Maneuvers.Select(maneuver => new RouteManeuverDto(
                maneuver.Position,
                maneuver.InstructionKey.Value,
                new GeoCoordinateDto(maneuver.Location.Latitude, maneuver.Location.Longitude),
                maneuver.DistanceMetres,
                maneuver.Duration)).ToArray())).ToArray(),
        alternative.Duration,
        alternative.DistanceMetres,
        alternative.GeneralizedCostBreakdown
            .Select(component => new RouteCostComponentDto(component.Code.Value, component.Value)).ToArray(),
        new RouteAccessibilitySummaryDto(
            Values(alternative.AccessibilitySummary.SatisfiedHardConstraints),
            Values(alternative.AccessibilitySummary.RelevantUnknowns),
            Values(alternative.AccessibilitySummary.ImportantFacts)),
        Values(alternative.PreferenceTradeoffs),
        new ConfidenceSummaryDto(
            alternative.ConfidenceSummary.State,
            alternative.ConfidenceSummary.Score,
            alternative.ConfidenceSummary.EvidenceCount,
            alternative.ConfidenceSummary.EvaluatedAt),
        alternative.SourceVersions.ToDictionary(pair => pair.Key.Value, pair => pair.Value),
        alternative.CreatedAt,
        alternative.ExpiresAt);

    private static GeometryDto ToGeometry(SpatialGeometry geometry) => new(
        geometry.Kind,
        geometry.Coordinates.Select(coordinate =>
            new GeoCoordinateDto(coordinate.Latitude, coordinate.Longitude)).ToArray());

    private static IReadOnlyList<string> Values(IEnumerable<Code> values) =>
        values.Select(value => value.Value).Order(StringComparer.Ordinal).ToArray();
}
