using System.Text.Json;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AB.SmokPrzewodnik.Infrastructure.Routing;

internal static class RoutePlanSnapshotSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static ValueConverter<RoutePlanRequest, string> RequestConverter { get; } =
        new(
            value => SerializeRequest(value),
            value => DeserializeRequest(value));

    public static ValueComparer<RoutePlanRequest> RequestComparer { get; } =
        new(
            (left, right) => ReferenceEquals(left, right) ||
                left != null && right != null && SerializeRequest(left) == SerializeRequest(right),
            value => value == null ? 0 : SerializeRequest(value).GetHashCode(StringComparison.Ordinal),
            value => value == null ? null! : DeserializeRequest(SerializeRequest(value)));

    public static ValueConverter<IReadOnlyList<RouteAlternative>, string> AlternativesConverter { get; } =
        new(
            value => SerializeAlternatives(value),
            value => DeserializeAlternatives(value));

    public static ValueComparer<IReadOnlyList<RouteAlternative>> AlternativesComparer { get; } =
        new(
            (left, right) => ReferenceEquals(left, right) ||
                left != null && right != null && SerializeAlternatives(left) == SerializeAlternatives(right),
            value => value == null ? 0 : SerializeAlternatives(value).GetHashCode(StringComparison.Ordinal),
            value => value == null ? null! : DeserializeAlternatives(SerializeAlternatives(value)));

    public static ValueConverter<RouteAlternative, string> AlternativeConverter { get; } =
        new(
            value => SerializeAlternative(value),
            value => DeserializeAlternative(value));

    public static ValueComparer<RouteAlternative> AlternativeComparer { get; } =
        new(
            (left, right) => ReferenceEquals(left, right) ||
                left != null && right != null &&
                SerializeAlternative(left) == SerializeAlternative(right),
            value => value == null ? 0 : SerializeAlternative(value).GetHashCode(StringComparison.Ordinal),
            value => value == null ? null! : DeserializeAlternative(SerializeAlternative(value)));

    private static string SerializeAlternative(RouteAlternative alternative) =>
        SerializeAlternatives(new[] { alternative });

    private static RouteAlternative DeserializeAlternative(string json) =>
        DeserializeAlternatives(json).Single();

    private static string SerializeRequest(RoutePlanRequest request) =>
        JsonSerializer.Serialize(ToDocument(request), JsonOptions);

    private static RoutePlanRequest DeserializeRequest(string json) =>
        FromDocument(JsonSerializer.Deserialize<RoutePlanRequestDocument>(json, JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize a route-plan request."));

    private static string SerializeAlternatives(IReadOnlyList<RouteAlternative> alternatives) =>
        JsonSerializer.Serialize(alternatives.Select(ToDocument).ToArray(), JsonOptions);

    private static IReadOnlyList<RouteAlternative> DeserializeAlternatives(string json) =>
        (JsonSerializer.Deserialize<RouteAlternativeDocument[]>(json, JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize route alternatives."))
        .Select(FromDocument)
        .ToArray();

    private static RoutePlanRequestDocument ToDocument(RoutePlanRequest request) =>
        new(
            ToDocument(request.Origin),
            ToDocument(request.Destination),
            request.TravelModes.Order().ToArray(),
            new SortedDictionary<string, ConstraintLevel>(
                request.Constraints.ToDictionary(pair => pair.Key.Value, pair => pair.Value),
                StringComparer.Ordinal),
            request.RequestedProfiles.Select(code => code.Value).ToArray(),
            request.Locale,
            new SortedDictionary<string, string>(
                request.ClientDataVersions.ToDictionary(pair => pair.Key.Value, pair => pair.Value),
                StringComparer.Ordinal));

    private static RoutePlanRequest FromDocument(RoutePlanRequestDocument document) =>
        new(
            FromDocument(document.Origin),
            FromDocument(document.Destination),
            document.TravelModes,
            document.Constraints.ToDictionary(pair => new Code(pair.Key), pair => pair.Value),
            document.RequestedProfiles.Select(value => new Code(value)),
            document.Locale,
            document.ClientDataVersions.ToDictionary(pair => new Code(pair.Key), pair => pair.Value));

    private static RouteEndpointDocument ToDocument(RouteEndpoint endpoint) => endpoint switch
    {
        RouteEndpoint.Coordinate coordinate => new("coordinate", ToDocument(coordinate.Value), null),
        RouteEndpoint.Entity entity => new("entity", null, entity.EntityId),
        _ => throw new ArgumentOutOfRangeException(nameof(endpoint))
    };

    private static RouteEndpoint FromDocument(RouteEndpointDocument document) => document.Kind switch
    {
        "coordinate" when document.Coordinate is not null =>
            new RouteEndpoint.Coordinate(FromDocument(document.Coordinate)),
        "entity" when document.EntityId.HasValue => new RouteEndpoint.Entity(document.EntityId.Value),
        _ => throw new InvalidOperationException($"Unsupported persisted route endpoint '{document.Kind}'.")
    };

    private static RouteAlternativeDocument ToDocument(RouteAlternative alternative) =>
        new(
            alternative.Id,
            alternative.LabelKey.Value,
            ToDocument(alternative.Geometry),
            alternative.Legs.Select(ToDocument).ToArray(),
            alternative.Duration,
            alternative.DistanceMetres,
            alternative.GeneralizedCostBreakdown
                .Select(component => new CostComponentDocument(component.Code.Value, component.Value))
                .ToArray(),
            new AccessibilitySummaryDocument(
                alternative.AccessibilitySummary.SatisfiedHardConstraints
                    .Select(code => code.Value).Order(StringComparer.Ordinal).ToArray(),
                alternative.AccessibilitySummary.RelevantUnknowns
                    .Select(code => code.Value).Order(StringComparer.Ordinal).ToArray(),
                alternative.AccessibilitySummary.ImportantFacts
                    .Select(code => code.Value).Order(StringComparer.Ordinal).ToArray()),
            alternative.PreferenceTradeoffs.Select(code => code.Value).Order(StringComparer.Ordinal).ToArray(),
            new ConfidenceDocument(
                alternative.ConfidenceSummary.State,
                alternative.ConfidenceSummary.Score,
                alternative.ConfidenceSummary.EvidenceCount,
                alternative.ConfidenceSummary.EvaluatedAt),
            new SortedDictionary<string, string>(
                alternative.SourceVersions.ToDictionary(pair => pair.Key.Value, pair => pair.Value),
                StringComparer.Ordinal),
            alternative.CreatedAt,
            alternative.ExpiresAt);

    private static RouteAlternative FromDocument(RouteAlternativeDocument document) =>
        new(
            document.Id,
            new Code(document.LabelKey),
            FromDocument(document.Geometry),
            document.Legs.Select(FromDocument),
            document.Duration,
            document.DistanceMetres,
            document.GeneralizedCostBreakdown.Select(component =>
                new CostComponent(new Code(component.Code), component.Value)),
            new AccessibilitySummary(
                document.AccessibilitySummary.SatisfiedHardConstraints.Select(value => new Code(value)),
                document.AccessibilitySummary.RelevantUnknowns.Select(value => new Code(value)),
                document.AccessibilitySummary.ImportantFacts.Select(value => new Code(value))),
            document.PreferenceTradeoffs.Select(value => new Code(value)),
            new ConfidenceAssessment(
                document.Confidence.State,
                document.Confidence.Score,
                document.Confidence.EvidenceCount,
                document.Confidence.EvaluatedAt),
            document.SourceVersions.ToDictionary(pair => new Code(pair.Key), pair => pair.Value),
            document.CreatedAt,
            document.ExpiresAt);

    private static RouteLegDocument ToDocument(RouteLeg leg) =>
        new(
            leg.Id,
            leg.Position,
            ToDocument(leg.Geometry),
            leg.Duration,
            leg.DistanceMetres,
            leg.Maneuvers.Select(maneuver => new ManeuverDocument(
                maneuver.Position,
                maneuver.InstructionKey.Value,
                ToDocument(maneuver.Location),
                maneuver.DistanceMetres,
                maneuver.Duration)).ToArray());

    private static RouteLeg FromDocument(RouteLegDocument document) =>
        new(
            document.Id,
            document.Position,
            FromDocument(document.Geometry),
            document.Duration,
            document.DistanceMetres,
            document.Maneuvers.Select(maneuver => new Maneuver(
                maneuver.Position,
                new Code(maneuver.InstructionKey),
                FromDocument(maneuver.Location),
                maneuver.DistanceMetres,
                maneuver.Duration)));

    private static GeometryDocument ToDocument(SpatialGeometry geometry) =>
        new(geometry.Kind, geometry.Coordinates.Select(ToDocument).ToArray());

    private static SpatialGeometry FromDocument(GeometryDocument document) =>
        new(document.Kind, document.Coordinates.Select(FromDocument));

    private static CoordinateDocument ToDocument(GeoCoordinate coordinate) =>
        new(coordinate.Latitude, coordinate.Longitude);

    private static GeoCoordinate FromDocument(CoordinateDocument coordinate) =>
        new(coordinate.Latitude, coordinate.Longitude);

    private sealed record RoutePlanRequestDocument(
        RouteEndpointDocument Origin,
        RouteEndpointDocument Destination,
        IReadOnlyList<TravelMode> TravelModes,
        IReadOnlyDictionary<string, ConstraintLevel> Constraints,
        IReadOnlyList<string> RequestedProfiles,
        string Locale,
        IReadOnlyDictionary<string, string> ClientDataVersions);

    private sealed record RouteEndpointDocument(
        string Kind,
        CoordinateDocument? Coordinate,
        Guid? EntityId);

    private sealed record RouteAlternativeDocument(
        Guid Id,
        string LabelKey,
        GeometryDocument Geometry,
        IReadOnlyList<RouteLegDocument> Legs,
        TimeSpan Duration,
        decimal DistanceMetres,
        IReadOnlyList<CostComponentDocument> GeneralizedCostBreakdown,
        AccessibilitySummaryDocument AccessibilitySummary,
        IReadOnlyList<string> PreferenceTradeoffs,
        ConfidenceDocument Confidence,
        IReadOnlyDictionary<string, string> SourceVersions,
        DateTimeOffset CreatedAt,
        DateTimeOffset ExpiresAt);

    private sealed record RouteLegDocument(
        Guid Id,
        int Position,
        GeometryDocument Geometry,
        TimeSpan Duration,
        decimal DistanceMetres,
        IReadOnlyList<ManeuverDocument> Maneuvers);

    private sealed record ManeuverDocument(
        int Position,
        string InstructionKey,
        CoordinateDocument Location,
        decimal DistanceMetres,
        TimeSpan Duration);

    private sealed record GeometryDocument(
        GeometryKind Kind,
        IReadOnlyList<CoordinateDocument> Coordinates);

    private sealed record CoordinateDocument(decimal Latitude, decimal Longitude);

    private sealed record CostComponentDocument(string Code, decimal Value);

    private sealed record AccessibilitySummaryDocument(
        IReadOnlyList<string> SatisfiedHardConstraints,
        IReadOnlyList<string> RelevantUnknowns,
        IReadOnlyList<string> ImportantFacts);

    private sealed record ConfidenceDocument(
        ConfidenceState State,
        decimal Score,
        int EvidenceCount,
        DateTimeOffset EvaluatedAt);
}
