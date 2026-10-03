using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Offline;

public sealed record SyncCommand
{
    public SyncCommand(string idempotencyKey, Code commandType, DateTimeOffset occurredAt, SyncPayload payload)
    {
        IdempotencyKey = Guard.NotBlank(idempotencyKey, nameof(idempotencyKey));
        CommandType = commandType;
        OccurredAt = occurredAt;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    public string IdempotencyKey { get; }
    public Code CommandType { get; }
    public DateTimeOffset OccurredAt { get; }
    public SyncPayload Payload { get; }
}
