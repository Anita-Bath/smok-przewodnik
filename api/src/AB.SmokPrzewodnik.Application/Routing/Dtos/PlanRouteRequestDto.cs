using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Routing.Dtos;

public sealed record PlanRouteRequestDto(
    RouteEndpointDto Origin,
    RouteEndpointDto Destination,
    IReadOnlyCollection<TravelMode> TravelModes,
    IReadOnlyDictionary<string, ConstraintLevel> Constraints,
    IReadOnlyCollection<string> RequestedProfiles,
    string Locale,
    IReadOnlyDictionary<string, string> ClientDataVersions);
