using System.Text.Json;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed class SourcePayload
{
    public SourcePayload(JsonElement value)
    {
        Value = value.Clone();
    }

    public JsonElement Value { get; }
}
