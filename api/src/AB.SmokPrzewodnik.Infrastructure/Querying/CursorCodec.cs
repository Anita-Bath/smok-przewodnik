using AB.SmokPrzewodnik.Application.Common.Querying;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AB.SmokPrzewodnik.Infrastructure.Querying;

internal sealed class CursorCodec : ICursorCodec
{
    private readonly string _signingKey;
    private static readonly JsonSerializerOptions _jsonOptions =
        new(JsonSerializerDefaults.Web);

    public CursorCodec(IConfiguration configuration)
    {
        _signingKey = configuration.GetSection("Pagination")
            .GetChildren()
            .First(x => x.Key == "SigningKey")
            .Value ?? string.Empty;
        ArgumentNullException.ThrowIfNullOrEmpty(_signingKey);
    }

    public string Encode<TCursor>(TCursor cursor)
    where TCursor : class, ICursorPayload
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(cursor, _jsonOptions);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingKey));
        var signature = hmac.ComputeHash(payload);

        return $"{Base64UrlEncode(payload)}.{Base64UrlEncode(signature)}";
    }

    public bool TryDecode<TCursor>(string encoded, string expectedScope, string expectedFilterHash, out TCursor? cursor)
    where TCursor : class, ICursorPayload
    {
        cursor = default;

        var parts = encoded.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        if (!TryBase64UrlDecode(parts[0], out var payload)
            || !TryBase64UrlDecode(parts[1], out var suppliedSignature))
        {
            return false;
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_signingKey));
        var expectedSignature = hmac.ComputeHash(payload);

        if (!CryptographicOperations.FixedTimeEquals(
            suppliedSignature,
            expectedSignature))
        {
            return false;
        }

        cursor = JsonSerializer.Deserialize<TCursor>(payload, _jsonOptions);

        return cursor is not null &&
                             cursor.Version == 1 &&
                             cursor.Scope == expectedScope &&
                             cursor.FilterHash == expectedFilterHash;
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
