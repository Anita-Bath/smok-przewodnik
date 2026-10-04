using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
using AB.SmokPrzewodnik.Application.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AB.SmokPrzewodnik.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(AssemblyReference.Assembly));

        services.AddScoped<INavigationTokenService, NavigationTokenService>();
        services.TryAddSingleton<INavigationEventPublisher, NullNavigationEventPublisher>();

        return services;
    }
}
