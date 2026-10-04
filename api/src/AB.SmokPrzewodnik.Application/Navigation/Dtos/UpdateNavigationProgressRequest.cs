using AB.SmokPrzewodnik.Application.Spatials.Dtos;

namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record UpdateNavigationProgressRequest(
    GeoCoordinateDto Position,
    decimal? AccuracyMetres,
    decimal? HeadingDegrees,
    DateTimeOffset RecordedAt,
    Guid? RouteLegId,
    int? ManeuverIndex);
