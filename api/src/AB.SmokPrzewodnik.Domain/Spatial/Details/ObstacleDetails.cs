using System.Collections.Frozen;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record ObstacleDetails : SpatialEntityDetails
{
    private ObstacleDetails() : base(EntityKind.Obstacle)
    {
        AffectedTravelModes = FrozenSet<TravelMode>.Empty;
    }

    public ObstacleDetails(
        Code obstacleCode,
        uint severity,
        DateTimeOffset? expectedUntil,
        IEnumerable<TravelMode> affectedTravelModes)
        : base(EntityKind.Obstacle)
    {
        ArgumentNullException.ThrowIfNull(affectedTravelModes);
        if (severity == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(severity));
        }

        var modes = affectedTravelModes.ToFrozenSet();
        if (modes.Count == 0)
        {
            throw new ArgumentException("At least one affected travel mode is required.", nameof(affectedTravelModes));
        }

        ObstacleCode = obstacleCode;
        Severity = severity;
        ExpectedUntil = expectedUntil;
        AffectedTravelModes = modes;
    }

    public Code ObstacleCode { get; private set; }
    public uint Severity { get; private set; }
    public DateTimeOffset? ExpectedUntil { get; private set; }
    public IReadOnlySet<TravelMode> AffectedTravelModes { get; private set; }
}
