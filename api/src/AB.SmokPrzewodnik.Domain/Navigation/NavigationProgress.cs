using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Navigation;

public sealed record NavigationProgress(
    GeoCoordinate Position,
    decimal? AccuracyMetres,
    decimal? HeadingDegrees,
    DateTimeOffset RecordedAt,
    Guid? RouteLegId,
    int? ManeuverIndex,
    decimal? RemainingDistanceMetres,
    bool IsOffRoute);
