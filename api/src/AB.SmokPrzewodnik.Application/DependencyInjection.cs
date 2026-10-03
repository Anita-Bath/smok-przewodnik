using Microsoft.Extensions.DependencyInjection;

namespace AB.SmokPrzewodnik.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
