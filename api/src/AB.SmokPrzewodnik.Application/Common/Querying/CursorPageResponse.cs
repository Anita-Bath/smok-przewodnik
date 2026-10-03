namespace AB.SmokPrzewodnik.Application.Common.Querying;

public sealed record CursorPageResponse<T>(
    IReadOnlyList<T> Items,
    string? NextCursor);
