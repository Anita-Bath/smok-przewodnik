using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Spatial;

public sealed class SpatialEntityTests
{
    [Fact]
    public void Create_GeneratesIdentityAndInitializesState()
    {
        var entity = SpatialEntity.Create(
            Guid.NewGuid(),
            EntityKind.Place,
            Point(),
            Confidence());

        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.Equal(LifecycleState.Active, entity.State);
        Assert.Equal(EntityKind.Place, entity.Kind);
    }

    [Fact]
    public void Constructor_PreservesRehydratedIdentity()
    {
        var id = Guid.NewGuid();

        var entity = new SpatialEntity(
            id,
            Guid.NewGuid(),
            EntityKind.Place,
            Point(),
            LifecycleState.Active,
            Confidence());

        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void SetDetails_RejectsMismatchedEntityKind()
    {
        var entity = CreatePlace();
        var details = new ObstacleDetails(new Code("stairs"), 3, null, new HashSet<TravelMode> { TravelMode.Walk });

        Assert.Throws<ArgumentException>(() => entity.SetDetails(details, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void UpsertTranslation_ReplacesLocaleCaseInsensitively()
    {
        var entity = CreatePlace();
        entity.UpsertTranslation(new EntityTranslation(Guid.NewGuid(), entity.Id, "pl-PL", "Wawel", "Opis"), DateTimeOffset.UtcNow);

        entity.UpsertTranslation(new EntityTranslation(Guid.NewGuid(), entity.Id, "PL-pl", "Wawel 2", "Nowy opis"), DateTimeOffset.UtcNow);

        var translation = Assert.Single(entity.Translations);
        Assert.Equal("Wawel 2", translation.Name);
    }

    [Fact]
    public void AddSourceLink_RejectsDuplicateSourceIdentity()
    {
        var entity = CreatePlace();
        var sourceId = Guid.NewGuid();
        entity.AddSourceLink(Link(entity.Id, sourceId, "node/1"), DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            entity.AddSourceLink(Link(entity.Id, sourceId, "node/1"), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Collections_CannotBeMutatedThroughExposedViews()
    {
        var entity = CreatePlace();

        Assert.True(Assert.IsAssignableFrom<ICollection<EntityTranslation>>(entity.Translations).IsReadOnly);
        Assert.True(Assert.IsAssignableFrom<ICollection<EntitySourceLink>>(entity.SourceLinks).IsReadOnly);
    }

    [Fact]
    public void ChangeLifecycleAndUpdateConfidence_ReplaceProjectedState()
    {
        var entity = CreatePlace();
        var at = DateTimeOffset.UtcNow.AddMinutes(1);
        var confidence = new ConfidenceAssessment(ConfidenceState.Disputed, 0.4m, 5, at);

        entity.ChangeLifecycle(LifecycleState.Closed, at);
        entity.UpdateConfidence(confidence, at);

        Assert.Equal(LifecycleState.Closed, entity.State);
        Assert.Equal(confidence, entity.Confidence);
        Assert.Equal(at, entity.UpdatedAt);
    }

    private static SpatialEntity CreatePlace() => SpatialEntity.Create(
        Guid.NewGuid(), EntityKind.Place, Point(), Confidence());

    private static SpatialGeometry Point() => new(GeometryKind.Point, [new GeoCoordinate(50, 19)]);

    private static ConfidenceAssessment Confidence() =>
        new(ConfidenceState.Unverified, 0, 0, DateTimeOffset.UtcNow);

    private static EntitySourceLink Link(Guid entityId, Guid sourceId, string externalId) => new(
        Guid.NewGuid(), entityId, sourceId, externalId, DateTimeOffset.UtcNow,
        new SourceLicenseMetadata(null, null, null), "v1");
}
