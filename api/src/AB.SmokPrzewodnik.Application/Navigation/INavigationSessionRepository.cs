using AB.SmokPrzewodnik.Application.Database;
using AB.SmokPrzewodnik.Domain.Navigation;

namespace AB.SmokPrzewodnik.Application.Navigation;

public interface INavigationSessionRepository
    : IRepository<NavigationSession, Guid, NavigationSessionCriteria>
{
    Task<NavigationSession> InsertAsync(string hash, Guid? accountId);
}
