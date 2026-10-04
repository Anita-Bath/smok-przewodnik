using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Routing.Dtos;

public sealed record RouteAlternativeDto(
    Guid Id,
    string LabelKey,
    GeometryDto Geometry,
    IReadOnlyList<RouteLegDto> Legs,
    TimeSpan Duration,
    decimal DistanceMetres,
    IReadOnlyList<RouteCostComponentDto> GeneralizedCostBreakdown,
    RouteAccessibilitySummaryDto AccessibilitySummary,
    IReadOnlyCollection<string> PreferenceTradeoffs,
    ConfidenceSummaryDto ConfidenceSummary,
    IReadOnlyDictionary<string, string> SourceVersions,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);

public sealed record RouteLegDto(
    Guid Id,
    int Position,
    GeometryDto Geometry,
    TimeSpan Duration,
    decimal DistanceMetres,
    IReadOnlyList<RouteManeuverDto> Maneuvers);

public sealed record RouteManeuverDto(
    int Position,
    string InstructionKey,
    GeoCoordinateDto Location,
    decimal DistanceMetres,
    TimeSpan Duration);

public sealed record RouteCostComponentDto(string Code, decimal Value);

public sealed record RouteAccessibilitySummaryDto(
    IReadOnlyCollection<string> SatisfiedHardConstraints,
    IReadOnlyCollection<string> RelevantUnknowns,
    IReadOnlyCollection<string> ImportantFacts);
