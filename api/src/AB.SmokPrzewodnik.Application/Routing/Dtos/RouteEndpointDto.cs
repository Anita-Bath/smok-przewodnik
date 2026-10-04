namespace AB.SmokPrzewodnik.Application.Routing.Dtos;

public sealed record RouteEndpointDto(
    decimal? Latitude,
    decimal? Longitude,
    Guid? EntityId);
