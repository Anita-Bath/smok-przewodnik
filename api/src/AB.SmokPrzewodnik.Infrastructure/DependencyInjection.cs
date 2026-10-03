using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
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
        services.AddSingleton<ICursorCodec, CursorCodec>();

        return services;
    }
}
