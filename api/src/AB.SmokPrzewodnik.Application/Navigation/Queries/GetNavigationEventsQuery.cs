using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Queries;

public sealed record GetNavigationEventsQuery(
    Guid SessionId,
    long AfterSequence = 0) : IRequest<NavigationEventsResponse>;
