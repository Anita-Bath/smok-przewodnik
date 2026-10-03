namespace AB.SmokPrzewodnik.Application.Common.Querying;

public sealed record CursorPageRequest
{
    public const int DefaultLimit = 10;
    public const int MaximumLimit = 200;

    public CursorPageRequest(
        string? cursor = null,
        int limit = DefaultLimit)
    {
        if (limit is < 1 or > MaximumLimit)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                $"Page size must be between 1 and {MaximumLimit}.");
        }

        Cursor = string.IsNullOrWhiteSpace(cursor)
            ? null
            : cursor;

        Limit = limit;
    }

    public string? Cursor { get; }

    public int Limit { get; }
}
