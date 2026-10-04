using AB.SmokPrzewodnik.Application.Routing.Dtos;

namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record RerouteNavigationSessionResponse(
    RouteAlternativeDto Route,
    long LatestSequence);
