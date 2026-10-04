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
        if (context.Resource is not HttpContext httpContext ||
            !Guid.TryParse(
                httpContext.Request.RouteValues["sessionId"]?.ToString(),
                out var requestedSessionId))
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
            httpContext.RequestAborted))
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
