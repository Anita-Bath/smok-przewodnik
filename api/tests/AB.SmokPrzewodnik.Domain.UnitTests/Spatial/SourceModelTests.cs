using System.Text.Json;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Spatial;

public sealed class SourceModelTests
{
    [Fact]
    public void GraphReference_WithEquivalentValues_HasValueEquality()
    {
        var left = new GraphReference("valhalla", "v1", GraphElementType.Edge, "42");
        var right = new GraphReference("valhalla", "v1", GraphElementType.Edge, "42");

        Assert.Equal(left, right);
    }

    [Fact]
    public void GraphReference_RejectsBlankProvider()
    {
        Assert.Throws<ArgumentException>(() =>
            new GraphReference(" ", "v1", GraphElementType.Edge, "42"));
    }

    [Fact]
    public void SourcePayload_OwnsJsonAfterDocumentIsDisposed()
    {
        SourcePayload payload;
        using (var document = JsonDocument.Parse("{\"wheelchair\":true}"))
        {
            payload = new SourcePayload(document.RootElement);
        }

        Assert.True(payload.Value.GetProperty("wheelchair").GetBoolean());
    }

    [Fact]
    public void SourceAssertion_RejectsReversedValidityRange()
    {
        var payload = ParsePayload("{}");

        Assert.Throws<ArgumentException>(() => new SourceAssertion(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "node/1",
            new Code("accessibility"),
            payload,
            DateTimeOffset.UtcNow,
            DateTimeOffset.Parse("2026-10-04T00:00:00Z"),
            DateTimeOffset.Parse("2026-10-03T00:00:00Z"),
            "v1"));
    }

    [Fact]
    public void EntitySourceLink_PreservesSourceIdentityAndLicense()
    {
        var license = new SourceLicenseMetadata("ODbL-1.0", "OpenStreetMap contributors", new Uri("https://www.openstreetmap.org/copyright"));
        var link = new EntitySourceLink(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "node/1",
            DateTimeOffset.UtcNow,
            license,
            "osm-v1");

        Assert.Equal("node/1", link.ExternalId);
        Assert.Equal(license, link.License);
        Assert.Equal("osm-v1", link.TransformVersion);
    }

    private static SourcePayload ParsePayload(string json)
    {
        using var document = JsonDocument.Parse(json);
        return new SourcePayload(document.RootElement);
    }
}
