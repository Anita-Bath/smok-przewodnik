namespace AB.SmokPrzewodnik.Application.Common.Querying;

public sealed record CursorPage<T>
{
    public CursorPage(
        IEnumerable<T> items,
        string? nextCursor)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (nextCursor is not null && string.IsNullOrWhiteSpace(nextCursor))
        {
            throw new ArgumentException(
                "The next cursor cannot be blank.",
                nameof(nextCursor));
        }

        Items = Array.AsReadOnly(items.ToArray());
        NextCursor = nextCursor;
    }

    public IReadOnlyList<T> Items { get; }

    public string? NextCursor { get; }

    public bool HasNextPage => NextCursor is not null;
}
