using AB.SmokPrzewodnik.Application.Navigation;

namespace AB.SmokPrzewodnik.Application.Auth.NavigationToken;

public interface INavigationTokenService
{
    Task<string> GetTokenAsync(Guid? accountId);
    Task<NavigationSessionDto?> GetSessionAsync(string token, CancellationToken cancellationToken);
    Task<bool> IsActiveAndOwnedByAsync(Guid sessionId,
        Guid accountId, CancellationToken cancellationToken);
}
