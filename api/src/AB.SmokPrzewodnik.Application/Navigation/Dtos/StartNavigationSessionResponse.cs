using AB.SmokPrzewodnik.Application.Routing.Dtos;

namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record StartNavigationSessionResponse(
    Guid SessionId,
    string NavigationToken,
    DateTimeOffset ExpiresAt,
    RouteAlternativeDto Route);
