using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Infrastructure.Navigation;
using AB.SmokPrzewodnik.Infrastructure.Querying;
using AB.SmokPrzewodnik.Infrastructure.Routing;
using AB.SmokPrzewodnik.Infrastructure.Routing.Valhalla;
using AB.SmokPrzewodnik.Infrastructure.Spatials;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AB.SmokPrzewodnik.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<Database.DbContext>();
        services.AddScoped<ISpatialEntityRepository, SpatialEntityRepository>();
        services.AddScoped<INavigationSessionRepository, NavigationSessionRepository>();
        services.AddScoped<INavigationProgressMatcher, NavigationProgressMatcher>();
        services.AddScoped<IRoutePlanRepository, RoutePlanRepository>();
        services.AddSingleton<ICursorCodec, CursorCodec>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(serviceProvider =>
        {
            var section = serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetSection(ValhallaOptions.SectionName);

            return new ValhallaOptions
            {
                BaseUrl = new Uri(section["BaseUrl"] ?? "http://localhost:8002"),
                TimeoutSeconds = section.GetValue("TimeoutSeconds", 10),
                Alternates = section.GetValue("Alternates", 2)
            };
        });
        services.AddHttpClient<IRoutePlanner, ValhallaRoutePlanner>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<ValhallaOptions>();
            client.BaseAddress = options.BaseUrl;
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });
        services.AddHostedService<NavigationCleanupService>();

        return services;
    }
}
