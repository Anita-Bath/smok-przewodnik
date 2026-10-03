using Microsoft.Extensions.DependencyInjection;

namespace AB.SmokPrzewodnik.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(AssemblyReference.Assembly));

        return services;
    }
}
