using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Queries;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Application.UnitTests.Spatials;

public sealed class GetSpatialEntitiesQueryHandlerTests
{
    [Fact]
    public async Task Handle_DelegatesPageAndCancellationAndMapsDto()
    {
        var createdAt = DateTimeOffset.Parse("2026-10-01T08:00:00Z");
        var updatedAt = DateTimeOffset.Parse("2026-10-02T09:30:00Z");
        var entityId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var cityId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var entity = new SpatialEntity(
            entityId,
            cityId,
            EntityKind.Obstacle,
            new SpatialGeometry(GeometryKind.Point, [new GeoCoordinate(50.0614m, 19.9366m)]),
            LifecycleState.Active,
            new ConfidenceAssessment(ConfidenceState.Supported, 0.85m, 12, updatedAt),
            createdAt,
            updatedAt);
        var pageRequest = new CursorPageRequest("cursor-one", 7);
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([entity], "cursor-two"));
        var handler = new GetSpatialEntitiesQueryHandler(repository);
        using var cancellation = new CancellationTokenSource();

        var response = await handler.Handle(
            new GetSpatialEntitiesQuery(pageRequest),
            cancellation.Token);

        Assert.Same(pageRequest, repository.Page);
        Assert.NotNull(repository.Criteria);
        Assert.Equal(cancellation.Token, repository.CancellationToken);
        Assert.Equal("cursor-two", response.NextCursor);

        var item = Assert.Single(response.Items);
        Assert.Equal(entityId, item.Id);
        Assert.Equal(cityId, item.CityId);
        Assert.Equal(EntityKind.Obstacle, item.Kind);
        Assert.Equal(LifecycleState.Active, item.State);
        Assert.Equal(GeometryKind.Point, item.Geometry.Kind);
        var coordinate = Assert.Single(item.Geometry.Coordinates);
        Assert.Equal(50.0614m, coordinate.Latitude);
        Assert.Equal(19.9366m, coordinate.Longitude);
        Assert.Equal(ConfidenceState.Supported, item.Confidence.State);
        Assert.Equal(0.85m, item.Confidence.Score);
        Assert.Equal(12, item.Confidence.EvidenceCount);
        Assert.Equal(updatedAt, item.Confidence.EvaluatedAt);
        Assert.Equal(createdAt, item.CreatedAt);
        Assert.Equal(updatedAt, item.UpdatedAt);
    }

    [Fact]
    public async Task Handle_MapsEmptyTerminalPage()
    {
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([], null));
        var handler = new GetSpatialEntitiesQueryHandler(repository);

        var response = await handler.Handle(
            new GetSpatialEntitiesQuery(new CursorPageRequest()),
            CancellationToken.None);

        Assert.Empty(response.Items);
        Assert.Null(response.NextCursor);
    }

    [Fact]
    public void ListItemDto_DoesNotExposeDomainValueObjectsOrNavigationCollections()
    {
        var propertyTypes = typeof(SpatialEntityListItemDto)
            .GetProperties()
            .Select(property => property.PropertyType)
            .ToArray();

        Assert.DoesNotContain(typeof(SpatialGeometry), propertyTypes);
        Assert.DoesNotContain(typeof(ConfidenceAssessment), propertyTypes);
        Assert.DoesNotContain(typeof(IReadOnlyCollection<EntityTranslation>), propertyTypes);
        Assert.DoesNotContain(typeof(IReadOnlyCollection<EntitySourceLink>), propertyTypes);
        Assert.DoesNotContain(typeof(IReadOnlyCollection<AccessibilityFact>), propertyTypes);
    }

    private sealed class FakeSpatialEntityRepository : ISpatialEntityRepository
    {
        private readonly CursorPage<SpatialEntity> _result;

        public FakeSpatialEntityRepository(CursorPage<SpatialEntity> result)
        {
            _result = result;
        }

        public SpatialEntityCriteria? Criteria { get; private set; }
        public CursorPageRequest? Page { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<SpatialEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<SpatialEntity?>(null);

        public Task<CursorPage<SpatialEntity>> FindAsync(
            SpatialEntityCriteria criteria,
            CursorPageRequest page,
            CancellationToken cancellationToken)
        {
            Criteria = criteria;
            Page = page;
            CancellationToken = cancellationToken;
            return Task.FromResult(_result);
        }

        public Task UpdateAsync(SpatialEntity aggregate, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
