using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed record DeleteNavigationSessionCommand(Guid SessionId) : IRequest;
