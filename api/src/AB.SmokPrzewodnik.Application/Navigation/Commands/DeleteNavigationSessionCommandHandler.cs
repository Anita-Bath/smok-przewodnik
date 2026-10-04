using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed class DeleteNavigationSessionCommandHandler(INavigationSessionRepository sessions)
    : IRequestHandler<DeleteNavigationSessionCommand>
{
    public async Task Handle(DeleteNavigationSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await sessions.GetByIdAsync(command.SessionId, cancellationToken)
            ?? throw new NavigationSessionNotFoundException(command.SessionId);
        await sessions.DeleteAsync(session, cancellationToken);
    }
}
