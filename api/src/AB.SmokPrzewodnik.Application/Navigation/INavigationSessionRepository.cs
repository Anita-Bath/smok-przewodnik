using AB.SmokPrzewodnik.Application.Database;
using AB.SmokPrzewodnik.Domain.Navigation;

namespace AB.SmokPrzewodnik.Application.Navigation;

public interface INavigationSessionRepository
    : IRepository<NavigationSession, Guid, NavigationSessionCriteria>
{
    Task AddAsync(NavigationSession session, CancellationToken cancellationToken);

    Task UpdateAsync(
        NavigationSession session,
        IReadOnlyCollection<NavigationEvent> events,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<NavigationEvent>> GetEventsAfterAsync(
        Guid sessionId,
        long afterSequence,
        int limit,
        CancellationToken cancellationToken);

    Task DeleteAsync(NavigationSession session, CancellationToken cancellationToken);

    Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken cancellationToken);
}
