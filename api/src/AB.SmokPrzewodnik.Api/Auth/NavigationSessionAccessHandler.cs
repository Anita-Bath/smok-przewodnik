using System.Security.Claims;
using AB.SmokPrzewodnik.Api.Auth.NavigationToken;
using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
using Microsoft.AspNetCore.Authorization;

namespace AB.SmokPrzewodnik.Api.Auth;

internal sealed class NavigationSessionAccessHandler(INavigationTokenService navTokenService) : AuthorizationHandler<NavigationSessionAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
          AuthorizationHandlerContext context,
          NavigationSessionAccessRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        var requestedSessionId = context.Resource switch
        {
            Guid id => id,
            HttpContext value when Guid.TryParse(
                value.Request.RouteValues["sessionId"]?.ToString(), out var id) => id,
            _ => Guid.Empty
        };
        if (requestedSessionId == Guid.Empty)
        {
            return;
        }

        if (HasMatchingNavigationToken(context.User, requestedSessionId))
        {
            context.Succeed(requirement);
            return;
        }

        var accountIdValue = context.User.FindFirst("sub")?.Value;
        var role = context.User.FindFirst("role")?.Value;

        if (role != requirement.AuthenticatedRole ||
            !Guid.TryParse(accountIdValue, out var accountId))
        {
            return;
        }

        if (await navTokenService.IsActiveAndOwnedByAsync(
            requestedSessionId,
            accountId,
            httpContext?.RequestAborted ?? CancellationToken.None))
        {
            context.Succeed(requirement);
        }
    }

    private static bool HasMatchingNavigationToken(
          ClaimsPrincipal principal,
          Guid requestedSessionId)
    {
        var navigationIdentity = principal.Identities.FirstOrDefault(
            identity => identity.AuthenticationType ==
                        NavigationTokenDefaults.AuthenticationScheme);

        return Guid.TryParse(
            navigationIdentity?.FindFirst("SessionId")?.Value,
            out var tokenSessionId) &&
               tokenSessionId == requestedSessionId;
    }
}
