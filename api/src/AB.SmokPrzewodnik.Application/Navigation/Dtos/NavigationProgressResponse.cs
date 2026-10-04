using AB.SmokPrzewodnik.Application.Spatials.Dtos;

namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record NavigationProgressResponse(
    GeoCoordinateDto Position,
    GeoCoordinateDto MatchedPosition,
    decimal RemainingDistanceMetres,
    bool IsOffRoute,
    bool WasIgnored,
    long LatestSequence);
