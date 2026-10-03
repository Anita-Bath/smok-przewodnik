using System.Text.Json;
using AB.SmokPrzewodnik.Domain.Offline;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Offline;

public sealed class OfflineContractTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public void Manifest_RequiresIdentifiersVersionsAndResources()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() => Manifest(Guid.Empty, "1.0.0", [Resource(now)]));
        Assert.Throws<ArgumentException>(() => Manifest(Guid.NewGuid(), "not-semver", [Resource(now)]));
        Assert.Throws<ArgumentException>(() => Manifest(Guid.NewGuid(), "1.0.0", []));
    }

    [Fact]
    public void Resource_ValidatesSizeHashAndExpiry()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentOutOfRangeException>(() => new AreaPackResource(
            new Code("map"), new Uri("https://example.test/map"), Hash, -1, now.AddHours(1)));
        Assert.Throws<ArgumentException>(() => new AreaPackResource(
            new Code("map"), new Uri("https://example.test/map"), Hash.ToUpperInvariant(), 1, now.AddHours(1)));
        Assert.Throws<ArgumentException>(() => Manifest(
            Guid.NewGuid(), "1.0.0", [Resource(now.AddHours(-1))], createdAt: now));
    }

    [Fact]
    public void Manifest_PreservesMinimumVersionAndRejectsSelfBasedDelta()
    {
        var now = DateTimeOffset.UtcNow;
        var manifest = Manifest(Guid.NewGuid(), "2.0.0", [Resource(now.AddHours(1))], "1.4.0", now);

        Assert.Equal("1.4.0", manifest.MinimumAppVersion);
        Assert.Throws<ArgumentException>(() => Manifest(
            Guid.NewGuid(), "2.0.0", [Resource(now.AddHours(1))], "2.0.0", now));
    }

    [Fact]
    public void SyncPayload_SurvivesSourceDocumentDisposal()
    {
        SyncPayload payload;
        using (var document = JsonDocument.Parse("{\"entityId\":\"abc\"}"))
        {
            payload = new SyncPayload(document.RootElement);
        }

        Assert.Equal("abc", payload.Value.GetProperty("entityId").GetString());
    }

    [Fact]
    public void SyncCommand_RequiresIdentityTypeAndNonEmptyPayload()
    {
        using var document = JsonDocument.Parse("null");

        Assert.Throws<ArgumentException>(() => new SyncCommand(
            " ", new Code("observation.create"), DateTimeOffset.UtcNow, new SyncPayload(document.RootElement)));
        Assert.Throws<ArgumentException>(() => new SyncCommand(
            "key", new Code("observation.create"), DateTimeOffset.UtcNow, new SyncPayload(document.RootElement)));
    }

    [Fact]
    public void SyncResult_EnforcesSuccessAndErrorShapes()
    {
        Assert.Throws<ArgumentException>(() => new SyncResult(
            "key", SyncStatus.Succeeded, null, null, []));
        Assert.Throws<ArgumentException>(() => new SyncResult(
            "key", SyncStatus.Rejected, null, null, []));
        Assert.Throws<ArgumentException>(() => new SyncResult(
            "key", SyncStatus.Succeeded, Guid.NewGuid(), 1, ["unexpected"]));
    }

    [Fact]
    public void SyncResult_CopiesValidationErrors()
    {
        var errors = new List<string> { "invalid target" };
        var result = new SyncResult("key", SyncStatus.Rejected, null, null, errors);
        errors.Add("later mutation");

        Assert.Single(result.ValidationErrors);
        Assert.True(Assert.IsAssignableFrom<ICollection<string>>(result.ValidationErrors).IsReadOnly);
    }

    private static AreaPackManifest Manifest(
        Guid cityId,
        string version,
        IEnumerable<AreaPackResource> resources,
        string? baseVersion = null,
        DateTimeOffset? createdAt = null) =>
        new(
            cityId,
            Guid.NewGuid(),
            version,
            createdAt ?? DateTimeOffset.UtcNow,
            "1.4.0",
            resources,
            baseVersion);

    private static AreaPackResource Resource(DateTimeOffset expiresAt) =>
        new(new Code("routing_graph"), new Uri("https://example.test/pack"), Hash, 1024, expiresAt);
}
