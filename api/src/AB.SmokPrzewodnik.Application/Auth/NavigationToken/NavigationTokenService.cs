using System.Security.Cryptography;
using System.Text;
using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Navigation;
using AB.SmokPrzewodnik.Application.Navigation.Mappers;

namespace AB.SmokPrzewodnik.Application.Auth.NavigationToken;

internal sealed class NavigationTokenService : INavigationTokenService
{
    private readonly INavigationSessionRepository _repository;

    public NavigationTokenService(INavigationSessionRepository repository)
    {
        _repository = repository;
    }

    public async Task<NavigationSessionDto?> GetSessionAsync(string token, CancellationToken cancellationToken)
    {
        var hash = HashToken(token);

        var criteria = new NavigationSessionCriteria(hash);
        var page = new CursorPageRequest(null, null);

        var session = await _repository.FindAsync(criteria, page, cancellationToken);

        if (session.Items.Count == 0)
        {
            return null;
        }

        return NavigationSessionMapper.ToDto(session.Items.Single());
    }

    public async Task<string> GetTokenAsync(Guid? accountId)
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Base64UrlEncode(tokenBytes);
        var tokenHash = HashToken(token);

        await _repository.InsertAsync(tokenHash, accountId);
        return token;
    }

    public async Task<bool> IsActiveAndOwnedByAsync(Guid sessionId,
        Guid accountId, CancellationToken cancellationToken)
    {
        var criteria = new NavigationSessionCriteria(null, accountId, sessionId);
        var page = new CursorPageRequest(null, null);

        var session = await _repository.FindAsync(criteria, page, cancellationToken);

        if (session.Items.Count == 0)
        {
            return false;
        }

        return true;
    }

    private static string HashToken(string token)
    {
        return Base64UrlEncode(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToBase64String(bytes)
          .TrimEnd('=')
          .Replace('+', '-')
          .Replace('/', '_');
    }

    private static bool TryBase64UrlDecode(
          string value,
          out byte[] bytes)
    {
        bytes = [];

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var base64 = value
            .Replace('-', '+')
            .Replace('_', '/');

        base64 = ((base64.Length) % 4) switch
        {
            0 => base64,
            2 => base64 + "==",
            3 => base64 + "=",
            _ => string.Empty
        };

        if (base64.Length == 0)
        {
            return false;
        }

        try
        {
            bytes = Convert.FromBase64String(base64);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
