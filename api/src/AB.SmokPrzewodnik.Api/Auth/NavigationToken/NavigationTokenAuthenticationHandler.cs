using System.Security.Claims;
using System.Text.Encodings.Web;
using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AB.SmokPrzewodnik.Api.Auth.NavigationToken;

public sealed class NavigationTokenAuthenticationHandler : AuthenticationHandler<NavigationTokenAuthenticationOptions>
{
    private readonly INavigationTokenService _navTokenService;

    public NavigationTokenAuthenticationHandler(IOptionsMonitor<NavigationTokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, INavigationTokenService navTokenService) : base(options, logger, encoder)
    {
        _navTokenService = navTokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = Request.Headers?[Options.HttpHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(token))
        {
            return AuthenticateResult.NoResult();
        }

        var session = await _navTokenService.GetSessionAsync(token, Context.RequestAborted);

        if (session is null)
        {
            return AuthenticateResult.Fail("Invalid navigation token");
        }

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.AuthenticationMethod, Scheme.Name),
                new Claim("SessionId", session.Id.ToString())
            };

        if (session.AccountId is { } accountId)
        {
            claims.Add(new Claim("AccountId", accountId.ToString()));
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }
}
