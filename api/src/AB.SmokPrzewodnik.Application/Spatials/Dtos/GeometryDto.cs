using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record GeometryDto(
    GeometryKind Kind,
    IReadOnlyList<GeoCoordinateDto> Coordinates);

public sealed record GeoCoordinateDto(
    decimal Latitude,
    decimal Longitude);
