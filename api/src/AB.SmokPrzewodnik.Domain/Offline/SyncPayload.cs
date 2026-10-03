using System.Text.Json;

namespace AB.SmokPrzewodnik.Domain.Offline;

public sealed record SyncPayload
{
    public SyncPayload(JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            throw new ArgumentException("A synchronization payload must contain a JSON value.", nameof(value));
        }

        Value = value.Clone();
    }

    public JsonElement Value { get; }
}
