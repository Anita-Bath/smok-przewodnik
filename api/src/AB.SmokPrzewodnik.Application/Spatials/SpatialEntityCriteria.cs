using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Spatials;

public sealed record SpatialEntityCriteria
{
    public SpatialEntityCriteria(
        EntityKind? kind = null,
        BoundingBox? boundingBox = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        IEnumerable<string>? categories = null,
        bool fromIsDynamic = false)
    {
        if (from.HasValue && to < from)
        {
            throw new ArgumentException("The end of the time range cannot precede its start.", nameof(to));
        }

        Kind = kind;
        BoundingBox = boundingBox;
        From = from;
        To = to;
        FromIsDynamic = fromIsDynamic;
        Categories = (categories ?? [])
            .Select(category => category.Trim().ToLowerInvariant())
            .Where(category => category.Length > 0)
            .ToFrozenSet(StringComparer.Ordinal);
    }

    public EntityKind? Kind { get; }
    public BoundingBox? BoundingBox { get; }
    public DateTimeOffset? From { get; }
    public DateTimeOffset? To { get; }
    public bool FromIsDynamic { get; }
    public IReadOnlySet<string> Categories { get; }
}

public sealed record BoundingBox
{
    public BoundingBox(
        decimal minLongitude,
        decimal minLatitude,
        decimal maxLongitude,
        decimal maxLatitude)
    {
        if (minLongitude is < -180 or > 180 || maxLongitude is < -180 or > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(minLongitude), "Longitude must be between -180 and 180.");
        }

        if (minLatitude is < -90 or > 90 || maxLatitude is < -90 or > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(minLatitude), "Latitude must be between -90 and 90.");
        }

        if (minLongitude >= maxLongitude || minLatitude >= maxLatitude)
        {
            throw new ArgumentException("Bounding-box minimums must be lower than maximums.");
        }

        MinLongitude = minLongitude;
        MinLatitude = minLatitude;
        MaxLongitude = maxLongitude;
        MaxLatitude = maxLatitude;
    }

    public decimal MinLongitude { get; }
    public decimal MinLatitude { get; }
    public decimal MaxLongitude { get; }
    public decimal MaxLatitude { get; }
}
