using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed record RerouteNavigationSessionCommand(
    Guid SessionId,
    Guid RoutePlanId,
    Guid RouteAlternativeId) : IRequest<RerouteNavigationSessionResponse>;
