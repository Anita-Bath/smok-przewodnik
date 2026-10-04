using Microsoft.AspNetCore.Authentication;

namespace AB.SmokPrzewodnik.Api.Auth.NavigationToken;

public static class NavigationTokenAuthExtension
{
    public static AuthenticationBuilder AddNavigationToken(this AuthenticationBuilder builder)
    {
        builder.AddScheme<NavigationTokenAuthenticationOptions, NavigationTokenAuthenticationHandler>(NavigationTokenDefaults.AuthenticationScheme, options => {});

        return builder;
    }
}
