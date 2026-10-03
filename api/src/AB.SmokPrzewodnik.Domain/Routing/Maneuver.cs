using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Routing;

public sealed record Maneuver
{
    public Maneuver(
        int position,
        Code instructionKey,
        GeoCoordinate location,
        decimal distanceMetres,
        TimeSpan duration)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (distanceMetres < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(distanceMetres));
        }

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        Position = position;
        InstructionKey = instructionKey;
        Location = location;
        DistanceMetres = distanceMetres;
        Duration = duration;
    }

    public int Position { get; }

    public Code InstructionKey { get; }

    public GeoCoordinate Location { get; }

    public decimal DistanceMetres { get; }

    public TimeSpan Duration { get; }
}
