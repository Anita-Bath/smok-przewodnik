using System.Net;
using System.Net.Http.Json;
using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests;

public sealed class SpatialsEndpointTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-03T12:00:00Z");

    [Fact]
    public async Task GetEntities_UsesDefaultLimitAndReturnsMappedPage()
    {
        var repository = new FakeSpatialEntityRepository(PageWithEntity("next-page"));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync("/v1/spatials/entities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<CursorPageResponse<SpatialEntityListItemDto>>();
        Assert.NotNull(page);
        Assert.Equal("next-page", page.NextCursor);
        Assert.Single(page.Items);
        Assert.Equal(10, repository.Page?.Limit);
        Assert.Null(repository.Page?.Cursor);
    }

    [Fact]
    public async Task GetEntities_ForwardsExplicitCursorAndLimit()
    {
        var repository = new FakeSpatialEntityRepository(new CursorPage<SpatialEntity>([], null));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync("/v1/spatials/entities?cursor=page-one&limit=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("page-one", repository.Page?.Cursor);
        Assert.Equal(3, repository.Page?.Limit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task GetEntities_WithInvalidLimit_ReturnsProblemDetails(int limit)
    {
        var repository = new FakeSpatialEntityRepository(new CursorPage<SpatialEntity>([], null));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync($"/v1/spatials/entities?limit={limit}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(repository.Page);
    }

    [Fact]
    public async Task GetEntities_WithInvalidCursor_ReturnsProblemDetails()
    {
        var repository = new FakeSpatialEntityRepository(new CursorPage<SpatialEntity>([], null));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync("/v1/spatials/entities?cursor=invalid");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetPlace_ReturnsPlaceDetails()
    {
        var place = CreatePlace();
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([], null),
            place);
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync($"/v1/places/{place.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<PlaceDto>();
        Assert.NotNull(dto);
        Assert.Equal("museum", dto.CategoryCode);
        Assert.Equal("Muzeum", Assert.Single(dto.Translations).Name);
    }

    [Fact]
    public async Task GetPlace_WhenEntityIsNotAPlace_ReturnsNotFound()
    {
        var eventEntity = CreateEvent();
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([], null),
            eventEntity);
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync($"/v1/places/{eventEntity.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_ReturnsEventDetails()
    {
        var eventEntity = CreateEvent();
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([], null),
            eventEntity);
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync($"/v1/events/{eventEntity.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(dto);
        Assert.Equal(["concert", "workshop"], dto.CategoryCodes);
    }

    [Fact]
    public async Task GetEvents_ParsesFiltersAndReturnsCursorPage()
    {
        var eventEntity = CreateEvent();
        var repository = new FakeSpatialEntityRepository(
            new CursorPage<SpatialEntity>([eventEntity], "next-events"));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync(
            "/v1/events?bbox=19.8,49.9,20.1,50.2&from=2026-10-03T10:00:00Z&to=2026-10-04T10:00:00Z&categories=Workshop,concert&cursor=page-one&limit=4");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<CursorPageResponse<EventDto>>();
        Assert.NotNull(page);
        Assert.Single(page.Items);
        Assert.Equal("next-events", page.NextCursor);
        Assert.Equal("page-one", repository.Page?.Cursor);
        Assert.Equal(4, repository.Page?.Limit);
        Assert.NotNull(repository.Criteria);
        Assert.Equal(EntityKind.Event, repository.Criteria.Kind);
        Assert.Equal(DateTimeOffset.Parse("2026-10-03T10:00:00Z"), repository.Criteria.From);
        Assert.False(repository.Criteria.FromIsDynamic);
        Assert.Equal(DateTimeOffset.Parse("2026-10-04T10:00:00Z"), repository.Criteria.To);
        Assert.Equal(["concert", "workshop"], repository.Criteria.Categories.Order().ToArray());
        Assert.Equal(19.8m, repository.Criteria.BoundingBox?.MinLongitude);
    }

    [Fact]
    public async Task GetEvents_WithoutFrom_UsesRequestTimeUtc()
    {
        var repository = new FakeSpatialEntityRepository(new CursorPage<SpatialEntity>([], null));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync("/v1/events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Now, repository.Criteria?.From);
        Assert.True(repository.Criteria?.FromIsDynamic);
    }

    [Theory]
    [InlineData("?bbox=19,50,18,51")]
    [InlineData("?bbox=19,50,20")]
    [InlineData("?from=2026-10-04T10:00:00Z&to=2026-10-03T10:00:00Z")]
    [InlineData("?categories=,,")]
    [InlineData("?limit=201")]
    public async Task GetEvents_WithInvalidQuery_ReturnsProblemDetails(string query)
    {
        var repository = new FakeSpatialEntityRepository(new CursorPage<SpatialEntity>([], null));
        await using var application = CreateApplication(repository);
        using var client = application.CreateClient();

        var response = await client.GetAsync($"/v1/events{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.Null(repository.Criteria);
    }

    private static WebApplicationFactory<Program> CreateApplication(
        FakeSpatialEntityRepository repository) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Pagination:SigningKey", "integration-test-signing-key");
            builder.UseSetting("ConnectionStrings:Default", "Host=localhost;Database=unused");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ISpatialEntityRepository>();
                services.AddSingleton<ISpatialEntityRepository>(repository);
                services.RemoveAll<TimeProvider>();
                services.AddSingleton<TimeProvider>(new FixedTimeProvider(Now));
            });
        });

    private static SpatialEntity CreatePlace()
    {
        var entity = CreateEntity(EntityKind.Place);
        entity.SetDetails(new PlaceDetails(
            new Code("museum"),
            "09:00-17:00",
            new ContactDetails("123", "museum@example.com"),
            new Uri("https://example.com")), Now);
        entity.UpsertTranslation(
            new EntityTranslation(Guid.NewGuid(), entity.Id, "pl", "Muzeum", "Opis"),
            Now);
        return entity;
    }

    private static SpatialEntity CreateEvent()
    {
        var entity = CreateEntity(EntityKind.Event);
        entity.SetDetails(new EventDetails(
            [new Code("workshop"), new Code("concert")],
            null,
            Now.AddHours(1),
            Now.AddHours(3),
            new Uri("https://example.com/tickets"),
            100), Now);
        return entity;
    }

    private static SpatialEntity CreateEntity(EntityKind kind) =>
        new(
            Guid.NewGuid(),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            kind,
            new SpatialGeometry(GeometryKind.Point, [new GeoCoordinate(50.0614m, 19.9366m)]),
            LifecycleState.Active,
            new ConfidenceAssessment(ConfidenceState.Supported, 0.9m, 4, Now),
            Now,
            Now);

    private static CursorPage<SpatialEntity> PageWithEntity(string? nextCursor)
    {
        var at = DateTimeOffset.Parse("2026-10-03T10:00:00Z");
        var entity = new SpatialEntity(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            EntityKind.Place,
            new SpatialGeometry(GeometryKind.Point, [new GeoCoordinate(50.0614m, 19.9366m)]),
            LifecycleState.Active,
            new ConfidenceAssessment(ConfidenceState.Supported, 0.9m, 4, at),
            at,
            at);

        return new CursorPage<SpatialEntity>([entity], nextCursor);
    }

    private sealed class FakeSpatialEntityRepository : ISpatialEntityRepository
    {
        private readonly CursorPage<SpatialEntity> _page;
        private readonly SpatialEntity? _entity;

        public FakeSpatialEntityRepository(
            CursorPage<SpatialEntity> page,
            SpatialEntity? entity = null)
        {
            _page = page;
            _entity = entity;
        }

        public CursorPageRequest? Page { get; private set; }
        public SpatialEntityCriteria? Criteria { get; private set; }

        public Task<SpatialEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_entity?.Id == id ? _entity : null);

        public Task<CursorPage<SpatialEntity>> FindAsync(
            SpatialEntityCriteria criteria,
            CursorPageRequest page,
            CancellationToken cancellationToken)
        {
            Page = page;
            Criteria = criteria;
            if (page.Cursor == "invalid")
            {
                throw new InvalidCursorException();
            }

            return Task.FromResult(_page);
        }

        public Task UpdateAsync(SpatialEntity aggregate, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
