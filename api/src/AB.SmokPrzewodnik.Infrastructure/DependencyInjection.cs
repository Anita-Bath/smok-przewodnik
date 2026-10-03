using Microsoft.Extensions.DependencyInjection;

namespace AB.SmokPrzewodnik.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<Database.DbContext>();

        return services;
    }
}
