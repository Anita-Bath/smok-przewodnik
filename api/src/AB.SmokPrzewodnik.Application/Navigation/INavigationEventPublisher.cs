using AB.SmokPrzewodnik.Application.Navigation.Dtos;

namespace AB.SmokPrzewodnik.Application.Navigation;

public interface INavigationEventPublisher
{
    Task PublishAsync(Guid sessionId, NavigationEventDto navigationEvent, CancellationToken cancellationToken);
}

internal sealed class NullNavigationEventPublisher : INavigationEventPublisher
{
    public Task PublishAsync(Guid sessionId, NavigationEventDto navigationEvent, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
