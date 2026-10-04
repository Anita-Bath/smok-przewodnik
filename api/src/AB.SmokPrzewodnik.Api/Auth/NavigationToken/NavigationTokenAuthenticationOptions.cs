using Microsoft.AspNetCore.Authentication;

namespace AB.SmokPrzewodnik.Api.Auth.NavigationToken;

public sealed class NavigationTokenAuthenticationOptions : AuthenticationSchemeOptions
{
    public string HttpHeader { get; set; } = "X-Navigation-Token";
}
