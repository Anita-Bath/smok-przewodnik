using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Infrastructure.Navigation;
using AB.SmokPrzewodnik.Infrastructure.Querying;
using AB.SmokPrzewodnik.Infrastructure.Spatials;
using Microsoft.Extensions.DependencyInjection;

namespace AB.SmokPrzewodnik.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<Database.DbContext>();
        services.AddScoped<ISpatialEntityRepository, SpatialEntityRepository>();
        services.AddScoped<INavigationSessionRepository, NavigationSessionRepository>();
        services.AddSingleton<ICursorCodec, CursorCodec>();

        return services;
    }
}
