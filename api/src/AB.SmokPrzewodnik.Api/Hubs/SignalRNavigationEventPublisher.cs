using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace AB.SmokPrzewodnik.Api.Hubs;

internal sealed class SignalRNavigationEventPublisher(
    IHubContext<NavigationHub> hubContext,
    ILogger<SignalRNavigationEventPublisher> logger) : INavigationEventPublisher
{
    public async Task PublishAsync(
        Guid sessionId,
        NavigationEventDto navigationEvent,
        CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.Group(NavigationHub.GroupName(sessionId))
                .SendAsync("NavigationEvent", navigationEvent, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Realtime navigation event publication failed for session {SessionId}.", sessionId);
        }
    }
}
