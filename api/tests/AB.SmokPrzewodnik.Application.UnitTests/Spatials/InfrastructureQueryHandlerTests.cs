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

public sealed class InfrastructureQueryHandlerTests
{
    [Fact]
    public async Task Handle_ForwardsInfrastructureCriteriaAndMapsPage()
    {
        var createdAt = DateTimeOffset.Parse("2026-10-03T08:00:00Z");
        var updatedAt = DateTimeOffset.Parse("2026-10-04T09:30:00Z");
        var entity = CreateInfrastructure(createdAt, updatedAt);
        var pageRequest = new CursorPageRequest("current-infrastructure", 6);
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([entity], "next-infrastructure"));
        var handler = new GetInfrastructureListQueryHandler(repository);
        using var cancellation = new CancellationTokenSource();

        var response = await handler.Handle(
            new GetInfrastructureListQuery(pageRequest),
            cancellation.Token);

        Assert.Same(pageRequest, repository.Page);
        Assert.Equal(cancellation.Token, repository.CancellationToken);
        Assert.NotNull(repository.Criteria);
        Assert.Equal(EntityKind.Infrastructure, repository.Criteria.Kind);
        Assert.Null(repository.Criteria.BoundingBox);
        Assert.Null(repository.Criteria.From);
        Assert.Null(repository.Criteria.To);
        Assert.Empty(repository.Criteria.Categories);
        Assert.False(repository.Criteria.FromIsDynamic);
        Assert.Equal("next-infrastructure", response.NextCursor);

        var item = Assert.Single(response.Items);
        Assert.Equal(entity.Id, item.Id);
        Assert.Equal(entity.CityId, item.CityId);
        Assert.Equal(EntityKind.Infrastructure, item.Kind);
        Assert.Equal(LifecycleState.Active, item.State);
        Assert.Equal(new Code("lift"), item.InfrastructureCode);
        Assert.Equal(new Code("operational"), item.OperationalState);
        Assert.Equal("inspection-2026-10", item.MaintenanceReference);
        Assert.Equal(GeometryKind.Point, item.Geometry.Kind);
        var coordinate = Assert.Single(item.Geometry.Coordinates);
        Assert.Equal(50.0614m, coordinate.Latitude);
        Assert.Equal(19.9366m, coordinate.Longitude);
        Assert.Equal(ConfidenceState.Supported, item.Confidence.State);
        Assert.Equal(0.92m, item.Confidence.Score);
        Assert.Equal(8, item.Confidence.EvidenceCount);
        Assert.Equal(updatedAt, item.Confidence.EvaluatedAt);
        Assert.Equal(createdAt, item.CreatedAt);
        Assert.Equal(updatedAt, item.UpdatedAt);
    }

    [Fact]
    public async Task Handle_MapsEmptyTerminalPage()
    {
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([], null));
        var handler = new GetInfrastructureListQueryHandler(repository);

        var response = await handler.Handle(
            new GetInfrastructureListQuery(new CursorPageRequest()),
            CancellationToken.None);

        Assert.Empty(response.Items);
        Assert.Null(response.NextCursor);
    }

    [Fact]
    public async Task Handle_ThrowsWhenRepositoryReturnsNonInfrastructureEntity()
    {
        var at = DateTimeOffset.Parse("2026-10-04T09:30:00Z");
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([CreateEntity(EntityKind.Place, at, at)], null));
        var handler = new GetInfrastructureListQueryHandler(repository);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new GetInfrastructureListQuery(new CursorPageRequest()),
            CancellationToken.None));

        Assert.Equal(
            "The infrastructure repository returned a non-infrastructure entity.",
            exception.Message);
    }

    private static SpatialEntity CreateInfrastructure(
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        var entity = CreateEntity(EntityKind.Infrastructure, createdAt, updatedAt);
        entity.SetDetails(new InfrastructureDetails(
            new Code("lift"),
            new Code("operational"),
            "inspection-2026-10"), updatedAt);
        return entity;
    }

    private static SpatialEntity CreateEntity(
        EntityKind kind,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) => new(
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        kind,
        new SpatialGeometry(GeometryKind.Point, [new GeoCoordinate(50.0614m, 19.9366m)]),
        LifecycleState.Active,
        new ConfidenceAssessment(ConfidenceState.Supported, 0.92m, 8, updatedAt),
        createdAt,
        updatedAt);

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
