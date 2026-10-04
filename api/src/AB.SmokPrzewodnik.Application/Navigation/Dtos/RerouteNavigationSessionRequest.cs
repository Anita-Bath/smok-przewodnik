namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record RerouteNavigationSessionRequest(Guid RoutePlanId, Guid RouteAlternativeId);
