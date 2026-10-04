using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Application.Spatials.Queries;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Spatials;

public sealed class PlaceAndEventQueryHandlerTests
{
    [Fact]
    public async Task GetPlace_MapsPlaceDetailsAndTranslations()
    {
        var entity = CreatePlace();
        var repository = new FakeSpatialEntityRepository(entity);
        var handler = new GetPlaceQueryHandler(repository);

        var result = await handler.Handle(new GetPlaceQuery(entity.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, repository.RequestedId);
        Assert.Equal("toilet", result.CategoryCode);
        Assert.Equal("24/7", result.OpeningHours);
        Assert.Equal("+48123456789", result.Contact?.Phone);
        Assert.Equal("info@example.test", result.Contact?.Email);
        Assert.Equal(new Uri("https://example.test/place"), result.Website);
        var translation = Assert.Single(result.Translations);
        Assert.Equal("pl-PL", translation.Locale);
        Assert.Equal("Toaleta", translation.Name);
        Assert.Equal("Dostepna toaleta", translation.Description);
        var fact = Assert.Single(result.AccessibilityFacts);
        Assert.Equal("elevator_broken", fact.AttributeCode);
        Assert.Equal("boolean", fact.Value.Kind);
        Assert.True(fact.Value.BooleanValue);
        Assert.Equal(DateTimeOffset.Parse("2026-10-03T10:00:00Z"), fact.ObservedAt);
        Assert.Equal(0.5m, fact.ConfidenceWeight);
    }

    [Fact]
    public async Task GetPlace_ReturnsNullForWrongEntityKind()
    {
        var repository = new FakeSpatialEntityRepository(CreateEvent());
        var handler = new GetPlaceQueryHandler(repository);

        var result = await handler.Handle(new GetPlaceQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEvent_MapsEventDetailsAndCategories()
    {
        var entity = CreateEvent();
        var repository = new FakeSpatialEntityRepository(entity);
        var handler = new GetEventQueryHandler(repository);

        var result = await handler.Handle(new GetEventQuery(entity.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(["concert", "workshop"], result.CategoryCodes);
        Assert.Equal(DateTimeOffset.Parse("2026-10-10T18:00:00Z"), result.StartsAt);
        Assert.Equal(DateTimeOffset.Parse("2026-10-10T20:00:00Z"), result.EndsAt);
        Assert.Equal(new Uri("https://example.test/book"), result.BookingUri);
        Assert.Equal((uint)120, result.Capacity);
        Assert.Single(result.Translations);
    }

    [Fact]
    public async Task GetEvent_ReturnsNullForWrongEntityKind()
    {
        var repository = new FakeSpatialEntityRepository(CreatePlace());
        var handler = new GetEventQueryHandler(repository);

        var result = await handler.Handle(new GetEventQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetEvents_ForwardsNormalizedCriteriaAndMapsPage()
    {
        var entity = CreateEvent();
        var repository = new FakeSpatialEntityRepository(
            entity,
            new CursorPage<SpatialEntity>([entity], "next-events"));
        var handler = new GetEventsQueryHandler(repository);
        var page = new CursorPageRequest("current-events", 4);
        var boundingBox = new BoundingBox(19.8m, 49.9m, 20.2m, 50.2m);
        var from = DateTimeOffset.Parse("2026-10-01T00:00:00Z");
        var to = DateTimeOffset.Parse("2026-11-01T00:00:00Z");

        var result = await handler.Handle(
            new GetEventsQuery(boundingBox, from, to, ["Workshop", "concert"], page),
            CancellationToken.None);

        Assert.Equal("next-events", result.NextCursor);
        var item = Assert.Single(result.Items);
        Assert.Equal(entity.Id, item.Id);
        Assert.Equal(["concert", "workshop"], item.CategoryCodes);
        Assert.NotNull(repository.Criteria);
        Assert.Equal(EntityKind.Event, repository.Criteria.Kind);
        Assert.Equal(boundingBox, repository.Criteria.BoundingBox);
        Assert.Equal(from, repository.Criteria.From);
        Assert.Equal(to, repository.Criteria.To);
        Assert.Equal(["concert", "workshop"], repository.Criteria.Categories.Order());
        Assert.Same(page, repository.Page);
    }

    [Fact]
    public void BoundingBox_RejectsInvalidCoordinateOrder()
    {
        Assert.Throws<ArgumentException>(() => new BoundingBox(20m, 49m, 19m, 50m));
        Assert.Throws<ArgumentException>(() => new BoundingBox(19m, 51m, 20m, 50m));
    }

    private static SpatialEntity CreatePlace()
    {
        var at = DateTimeOffset.Parse("2026-10-03T10:00:00Z");
        var entity = CreateEntity(EntityKind.Place, at);
        entity.SetDetails(new PlaceDetails(
            new Code("toilet"),
            "24/7",
            new ContactDetails("+48123456789", "info@example.test"),
            new Uri("https://example.test/place")), at);
        entity.UpsertTranslation(new EntityTranslation(
            Guid.NewGuid(), entity.Id, "pl-PL", "Toaleta", "Dostepna toaleta"), at);
        entity.AddAccessibilityFact(new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.SpatialEntity(entity.Id),
            new Code("elevator_broken"),
            new AccessibilityValue.Boolean(true),
            new EvidenceReference.Observation(Guid.NewGuid()),
            at,
            at,
            null,
            0.5m), at);
        return entity;
    }

    private static SpatialEntity CreateEvent()
    {
        var at = DateTimeOffset.Parse("2026-10-03T10:00:00Z");
        var entity = CreateEntity(EntityKind.Event, at);
        entity.SetDetails(new EventDetails(
            [new Code("workshop"), new Code("concert")],
            null,
            DateTimeOffset.Parse("2026-10-10T18:00:00Z"),
            DateTimeOffset.Parse("2026-10-10T20:00:00Z"),
            new Uri("https://example.test/book"),
            120), at);
        entity.UpsertTranslation(new EntityTranslation(
            Guid.NewGuid(), entity.Id, "en-GB", "Inclusive concert", "Live music"), at);
        return entity;
    }

    private static SpatialEntity CreateEntity(EntityKind kind, DateTimeOffset at) => new(
        Guid.NewGuid(),
        Guid.NewGuid(),
        kind,
        new SpatialGeometry(GeometryKind.Point, [new GeoCoordinate(50.0614m, 19.9366m)]),
        LifecycleState.Active,
        new ConfidenceAssessment(ConfidenceState.Supported, 0.9m, 4, at),
        at,
        at);

    private sealed class FakeSpatialEntityRepository : ISpatialEntityRepository
    {
        private readonly SpatialEntity? _entity;
        private readonly CursorPage<SpatialEntity> _page;

        public FakeSpatialEntityRepository(
            SpatialEntity? entity,
            CursorPage<SpatialEntity>? page = null)
        {
            _entity = entity;
            _page = page ?? new CursorPage<SpatialEntity>([], null);
        }

        public Guid? RequestedId { get; private set; }
        public SpatialEntityCriteria? Criteria { get; private set; }
        public CursorPageRequest? Page { get; private set; }

        public Task<SpatialEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            RequestedId = id;
            return Task.FromResult(_entity);
        }

        public Task<CursorPage<SpatialEntity>> FindAsync(
            SpatialEntityCriteria criteria,
            CursorPageRequest page,
            CancellationToken cancellationToken)
        {
            Criteria = criteria;
            Page = page;
            return Task.FromResult(_page);
        }

        public Task UpdateAsync(SpatialEntity aggregate, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
