using AB.SmokPrzewodnik.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgis/postgis:17-3.5")
        .Build();
    private ServiceProvider? _services;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = _container.GetConnectionString(),
                ["Pagination:SigningKey"] = "postgresql-fixture-signing-key"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure();
        _services = services.BuildServiceProvider(validateScopes: true);

        await using var scope = _services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetServices<Microsoft.EntityFrameworkCore.DbContext>().SingleOrDefault()
            ?? ResolveConcreteContext(scope.ServiceProvider, services);
        await context.Database.MigrateAsync();
    }

    public IServiceScope CreateScope() =>
        (_services ?? throw new InvalidOperationException("The PostgreSQL fixture has not started."))
        .CreateScope();

    public async Task DisposeAsync()
    {
        if (_services is not null)
        {
            await _services.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    private static Microsoft.EntityFrameworkCore.DbContext ResolveConcreteContext(
        IServiceProvider serviceProvider,
        IServiceCollection services)
    {
        var contextType = services
            .Select(descriptor => descriptor.ServiceType)
            .Single(type => typeof(Microsoft.EntityFrameworkCore.DbContext).IsAssignableFrom(type));

        return (Microsoft.EntityFrameworkCore.DbContext)serviceProvider.GetRequiredService(contextType);
    }
}

[CollectionDefinition(Name)]
public sealed class PostgreSqlCollection : ICollectionFixture<PostgreSqlFixture>
{
    public const string Name = "PostgreSQL";
}
