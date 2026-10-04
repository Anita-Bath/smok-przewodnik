using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Infrastructure.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Navigation;

public sealed class NavigationCleanupServiceTests
{
    [Fact]
    public async Task RunOnce_RemovesExpiredPlansAndSessionsAtCurrentTime()
    {
        var now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");
        var plans = new PlanRepository();
        var sessions = new SessionRepository();
        var services = new ServiceCollection()
            .AddSingleton<IRoutePlanRepository>(plans)
            .AddSingleton<INavigationSessionRepository>(sessions)
            .BuildServiceProvider();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var service = new NavigationCleanupService(
            services.GetRequiredService<IServiceScopeFactory>(),
            configuration,
            new FixedTimeProvider(now),
            NullLogger<NavigationCleanupService>.Instance);

        await service.RunOnceAsync(CancellationToken.None);

        Assert.Equal(now, plans.DeletedAt);
        Assert.Equal(now, sessions.DeletedAt);
    }

    private sealed class PlanRepository : IRoutePlanRepository
    {
        public DateTimeOffset? DeletedAt { get; private set; }
        public Task<RoutePlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<RoutePlan?>(null);
        public Task AddAsync(RoutePlan routePlan, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) { DeletedAt = now; return Task.FromResult(2); }
    }

    private sealed class SessionRepository : INavigationSessionRepository
    {
        public DateTimeOffset? DeletedAt { get; private set; }
        public Task AddAsync(NavigationSession session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(NavigationSession session, IReadOnlyCollection<NavigationEvent> events, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(Guid sessionId, long afterSequence, int limit, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<NavigationEvent>>([]);
        public Task DeleteAsync(NavigationSession session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken) { DeletedAt = now; return Task.FromResult(3); }
        public Task<NavigationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult<NavigationSession?>(null);
        public Task<CursorPage<NavigationSession>> FindAsync(NavigationSessionCriteria criteria, CursorPageRequest page, CancellationToken cancellationToken) => Task.FromResult(new CursorPage<NavigationSession>([], null));
    }
}
