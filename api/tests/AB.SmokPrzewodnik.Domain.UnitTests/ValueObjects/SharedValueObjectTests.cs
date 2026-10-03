using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.ValueObjects;

public sealed class SharedValueObjectTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Code_RejectsBlankValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new Code(value));
    }

    [Theory]
    [InlineData(-90.01, 0)]
    [InlineData(90.01, 0)]
    [InlineData(0, -180.01)]
    [InlineData(0, 180.01)]
    public void GeoCoordinate_RejectsOutOfRangeValues(decimal latitude, decimal longitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GeoCoordinate(latitude, longitude));
    }

    [Fact]
    public void SpatialGeometry_RejectsEmptyCoordinates()
    {
        Assert.Throws<ArgumentException>(() => new SpatialGeometry(GeometryKind.Line, []));
    }

    [Fact]
    public void SpatialGeometry_RejectsOpenPolygon()
    {
        GeoCoordinate[] coordinates =
        [
            new(50, 19),
            new(50, 20),
            new(51, 20),
            new(51, 19),
        ];

        Assert.Throws<ArgumentException>(() => new SpatialGeometry(GeometryKind.Polygon, coordinates));
    }

    [Fact]
    public void LocalizedContent_RejectsEmptyContent()
    {
        Assert.Throws<ArgumentException>(() => new LocalizedContent(new Dictionary<string, string>()));
    }

    [Fact]
    public void ValueObjects_WithEquivalentValues_AreEqual()
    {
        var left = new Code("stairs");
        var right = new Code("stairs");

        Assert.Equal(left, right);
        Assert.Equal(new GeoCoordinate(50.0617m, 19.9373m), new GeoCoordinate(50.0617m, 19.9373m));
    }
}
