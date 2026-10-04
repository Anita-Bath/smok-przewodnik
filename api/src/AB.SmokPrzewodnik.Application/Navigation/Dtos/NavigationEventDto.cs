using AB.SmokPrzewodnik.Application.Routing.Dtos;

namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record NavigationEventDto(
    long Sequence,
    string EventType,
    string Urgency,
    RouteManeuverDto? Maneuver,
    string? HazardCode,
    string? LandmarkCode,
    decimal? RemainingDistanceMetres,
    IReadOnlyDictionary<string, string> LocalizedParameters,
    IReadOnlyCollection<string> SupportedFeedbackPatterns,
    DateTimeOffset CreatedAt);
