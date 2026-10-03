namespace AB.SmokPrzewodnik.Api.Extensions;

public static class HealthEndpointExtension
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health").AllowAnonymous();

        return endpoints;
    }
}
