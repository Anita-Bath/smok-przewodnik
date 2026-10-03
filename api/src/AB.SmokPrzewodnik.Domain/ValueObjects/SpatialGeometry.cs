using System.Collections.ObjectModel;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.ValueObjects;

public sealed class SpatialGeometry : IEquatable<SpatialGeometry>
{
    private readonly ReadOnlyCollection<GeoCoordinate> _coordinates;

    public SpatialGeometry(GeometryKind kind, IEnumerable<GeoCoordinate> coordinates)
    {
        ArgumentNullException.ThrowIfNull(coordinates);
        var values = coordinates.ToArray();

        if (values.Length == 0)
        {
            throw new ArgumentException("Geometry must contain coordinates.", nameof(coordinates));
        }

        if (kind == GeometryKind.Point && values.Length != 1)
        {
            throw new ArgumentException("Point geometry must contain exactly one coordinate.", nameof(coordinates));
        }

        if (kind == GeometryKind.Line && values.Length < 2)
        {
            throw new ArgumentException("Line geometry must contain at least two coordinates.", nameof(coordinates));
        }

        if (kind == GeometryKind.Polygon && (values.Length < 4 || values[0] != values[^1]))
        {
            throw new ArgumentException("Polygon geometry must contain a closed ring of at least four coordinates.", nameof(coordinates));
        }

        Kind = kind;
        _coordinates = Array.AsReadOnly(values);
    }

    public GeometryKind Kind { get; }

    public IReadOnlyList<GeoCoordinate> Coordinates => _coordinates;

    public bool Equals(SpatialGeometry? other) =>
        other is not null && Kind == other.Kind && _coordinates.SequenceEqual(other._coordinates);

    public override bool Equals(object? obj) => obj is SpatialGeometry other && Equals(other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Kind);
        foreach (var coordinate in _coordinates)
        {
            hash.Add(coordinate);
        }

        return hash.ToHashCode();
    }
}
