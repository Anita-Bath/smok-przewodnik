using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Routing.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Navigation;

namespace AB.SmokPrzewodnik.Application.Navigation.Mappers;

internal static class NavigationEventMapper
{
    public static NavigationEventDto ToDto(NavigationEvent item) => new(
        item.Sequence,
        item.EventType.ToString(),
        item.Urgency.ToString(),
        item.Maneuver is null ? null : new RouteManeuverDto(
            item.Maneuver.Position,
            item.Maneuver.InstructionKey.Value,
            new GeoCoordinateDto(item.Maneuver.Location.Latitude, item.Maneuver.Location.Longitude),
            item.Maneuver.DistanceMetres,
            item.Maneuver.Duration),
        item.HazardCode?.Value,
        item.LandmarkCode?.Value,
        item.RemainingDistanceMetres,
        item.LocalizedParameters,
        item.SupportedFeedbackPatterns.Select(pattern => pattern.Value).Order(StringComparer.Ordinal).ToArray(),
        item.CreatedAt);
}
