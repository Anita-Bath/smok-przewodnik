using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed record StartNavigationSessionCommand(
    Guid RoutePlanId,
    Guid RouteAlternativeId,
    Guid? AccountId) : IRequest<StartNavigationSessionResponse>;
