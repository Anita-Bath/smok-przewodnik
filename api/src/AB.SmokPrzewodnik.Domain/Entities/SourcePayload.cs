using System.Text.Json;

namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed record SourcePayload(JsonElement Value);
