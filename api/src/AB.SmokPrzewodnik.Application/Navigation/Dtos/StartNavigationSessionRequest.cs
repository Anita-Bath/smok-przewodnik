namespace AB.SmokPrzewodnik.Application.Navigation.Dtos;

public sealed record StartNavigationSessionRequest(Guid RoutePlanId, Guid RouteAlternativeId);
