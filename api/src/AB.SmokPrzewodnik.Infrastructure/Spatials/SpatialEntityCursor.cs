using AB.SmokPrzewodnik.Application.Common.Querying;

namespace AB.SmokPrzewodnik.Infrastructure.Spatials;

internal sealed record SpatialEntityCursor(
    int Version,
    string Scope,
    string FilterHash,
    DateTimeOffset LastModifiedAt,
    Guid LastId,
    DateTimeOffset? DynamicFrom) : ICursorPayload;
