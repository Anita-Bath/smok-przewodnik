using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed record RouteLeg
{
    public RouteLeg(
        Guid id,
        int position,
        SpatialGeometry geometry,
        TimeSpan duration,
        decimal distanceMetres,
        IEnumerable<Maneuver> maneuvers)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The route-leg identifier cannot be empty.", nameof(id));
        }

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        if (distanceMetres < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceMetres));
        }

        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(maneuvers);
        var orderedManeuvers = maneuvers.ToList();
        if (orderedManeuvers.Select(item => item.Position).Distinct().Count() != orderedManeuvers.Count ||
            orderedManeuvers.Where((item, index) => item.Position != index).Any())
        {
            throw new ArgumentException("Maneuvers must have unique, contiguous positions.", nameof(maneuvers));
        }

        Id = id;
        Position = position;
        Geometry = geometry;
        Duration = duration;
        DistanceMetres = distanceMetres;
        Maneuvers = orderedManeuvers.AsReadOnly();
    }

    public Guid Id { get; }

    public int Position { get; }

    public SpatialGeometry Geometry { get; }

    public TimeSpan Duration { get; }

    public decimal DistanceMetres { get; }

    public IReadOnlyList<Maneuver> Maneuvers { get; }
}
