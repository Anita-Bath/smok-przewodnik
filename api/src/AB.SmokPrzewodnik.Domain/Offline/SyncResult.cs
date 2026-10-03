using AB.SmokPrzewodnik.Domain.Common;

namespace AB.SmokPrzewodnik.Domain.Offline;

public sealed record SyncResult
{
    public SyncResult(
        string idempotencyKey,
        SyncStatus status,
        Guid? canonicalId,
        long? serverVersion,
        IEnumerable<string> validationErrors)
    {
        IdempotencyKey = Guard.NotBlank(idempotencyKey, nameof(idempotencyKey));
        ArgumentNullException.ThrowIfNull(validationErrors);
        var errors = validationErrors.Select(error => Guard.NotBlank(error, nameof(validationErrors))).ToList();

        if (canonicalId == Guid.Empty)
        {
            throw new ArgumentException("The canonical identifier cannot be empty.", nameof(canonicalId));
        }

        if (serverVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(serverVersion));
        }

        if (status == SyncStatus.Succeeded && (!canonicalId.HasValue || !serverVersion.HasValue || errors.Count > 0))
        {
            throw new ArgumentException("A successful result requires a canonical ID and server version, with no errors.", nameof(status));
        }

        if (status != SyncStatus.Succeeded && errors.Count == 0)
        {
            throw new ArgumentException("An unsuccessful result requires at least one validation error.", nameof(validationErrors));
        }

        Status = status;
        CanonicalId = canonicalId;
        ServerVersion = serverVersion;
        ValidationErrors = errors.AsReadOnly();
    }

    public string IdempotencyKey { get; }
    public SyncStatus Status { get; }
    public Guid? CanonicalId { get; }
    public long? ServerVersion { get; }
    public IReadOnlyList<string> ValidationErrors { get; }
}
