namespace AB.SmokPrzewodnik.Application.Routing.Dtos;

public sealed record RoutePlanResponse(
    Guid RoutePlanId,
    DateTimeOffset ExpiresAt,
    IReadOnlyList<RouteAlternativeDto> Alternatives);
