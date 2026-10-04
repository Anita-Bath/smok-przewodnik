using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AB.SmokPrzewodnik.Infrastructure.Navigation;

internal sealed class NavigationCleanupService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    TimeProvider timeProvider,
    ILogger<NavigationCleanupService> logger) : BackgroundService
{
    private TimeSpan Interval => TimeSpan.FromMinutes(
        Math.Max(1, configuration.GetValue("NavigationCleanup:IntervalMinutes", 15)));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, timeProvider, stoppingToken);
            await RunOnceAsync(stoppingToken);
        }
    }

    internal async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await RunCleanupAsync<IRoutePlanRepository>(
            repository => repository.DeleteExpiredAsync(now, cancellationToken),
            "route plans");
        await RunCleanupAsync<INavigationSessionRepository>(
            repository => repository.DeleteExpiredAsync(now, cancellationToken),
            "navigation sessions");
    }

    private async Task RunCleanupAsync<TRepository>(
        Func<TRepository, Task<int>> cleanup,
        string resourceName)
        where TRepository : notnull
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var deleted = await cleanup(scope.ServiceProvider.GetRequiredService<TRepository>());
            if (deleted > 0)
            {
                logger.LogInformation("Removed {Count} expired {ResourceName}.", deleted, resourceName);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Cleanup of expired {ResourceName} failed.", resourceName);
        }
    }
}
