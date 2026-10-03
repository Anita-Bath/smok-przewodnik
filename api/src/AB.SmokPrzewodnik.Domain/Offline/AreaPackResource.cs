using System.Text.RegularExpressions;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Offline;

public sealed partial record AreaPackResource
{
    public AreaPackResource(Code type, Uri uri, string sha256, long byteSize, DateTimeOffset? expiresAt)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException("The resource URI must be absolute.", nameof(uri));
        }

        if (byteSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteSize));
        }

        if (sha256 is null || !Sha256Pattern().IsMatch(sha256))
        {
            throw new ArgumentException("The checksum must be a lowercase hexadecimal SHA-256 value.", nameof(sha256));
        }

        Type = type;
        Uri = uri;
        Sha256 = sha256;
        ByteSize = byteSize;
        ExpiresAt = expiresAt;
    }

    public Code Type { get; }
    public Uri Uri { get; }
    public string Sha256 { get; }
    public long ByteSize { get; }
    public DateTimeOffset? ExpiresAt { get; }

    [GeneratedRegex("^[0-9a-f]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Pattern();
}
