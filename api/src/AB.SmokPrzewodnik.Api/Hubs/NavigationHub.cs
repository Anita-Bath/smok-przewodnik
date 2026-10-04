using AB.SmokPrzewodnik.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AB.SmokPrzewodnik.Api.Hubs;

[Authorize(Policy = AppConsts.NavigationConnectionPolicyName)]
public sealed class NavigationHub(IAuthorizationService authorizationService) : Hub
{
    public async Task Subscribe(Guid sessionId)
    {
        var authorization = await authorizationService.AuthorizeAsync(
            Context.User!, sessionId, AppConsts.NavigationAuthPolicyName);
        if (!authorization.Succeeded)
        {
            throw new HubException("Access to the navigation session was denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(sessionId));
    }

    public Task Unsubscribe(Guid sessionId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(sessionId));

    internal static string GroupName(Guid sessionId) => $"navigation:{sessionId:N}";
}
