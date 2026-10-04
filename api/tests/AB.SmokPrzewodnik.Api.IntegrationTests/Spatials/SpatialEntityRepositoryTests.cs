using AB.SmokPrzewodnik.Api.IntegrationTests.Infrastructure;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AB.SmokPrzewodnik.Api.IntegrationTests.Spatials;

[Collection(PostgreSqlCollection.Name)]
public sealed class SpatialEntityRepositoryTests(PostgreSqlFixture database)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-04T12:00:00Z");

    [Fact]
    public async Task Update_InsertsFactAddedToLoadedAggregate()
    {
        var city = new City(
            Guid.NewGuid(),
            new Code($"city-{Guid.NewGuid():N}"),
            new LocalizedContent(new Dictionary<string, string> { ["pl-PL"] = "Miasto" }),
            "pl-PL",
            "Europe/Warsaw",
            true);
        var place = new SpatialEntity(
            Guid.NewGuid(),
            city.Id,
            EntityKind.Place,
            new SpatialGeometry(GeometryKind.Point, [new GeoCoordinate(50.0614m, 19.9366m)]),
            LifecycleState.Active,
            new ConfidenceAssessment(ConfidenceState.Unverified, 0m, 0, Now),
            Now,
            Now);
        place.SetDetails(new PlaceDetails(new Code("test-place"), null, null, null), Now);

        using (var setupScope = database.CreateScope())
        {
            var context = database.GetDbContext(setupScope.ServiceProvider);
            context.Set<City>().Add(city);
            context.Set<SpatialEntity>().Add(place);
            await context.SaveChangesAsync();
        }

        var factId = Guid.NewGuid();
        using (var updateScope = database.CreateScope())
        {
            var repository = updateScope.ServiceProvider.GetRequiredService<ISpatialEntityRepository>();
            var aggregate = (await repository.GetByIdAsync(place.Id, CancellationToken.None))!;
            aggregate.AddAccessibilityFact(new AccessibilityFact(
                factId,
                new AccessibilityFactTarget.SpatialEntity(place.Id),
                new Code("elevator_broken"),
                new AccessibilityValue.Boolean(true),
                new EvidenceReference.Observation(Guid.NewGuid()),
                Now,
                Now,
                null,
                0.5m), Now);

            await repository.UpdateAsync(aggregate, CancellationToken.None);
        }

        using var verificationScope = database.CreateScope();
        var verificationContext = database.GetDbContext(verificationScope.ServiceProvider);
        var stored = await verificationContext.Set<AccessibilityFact>()
            .SingleAsync(fact => fact.Id == factId);

        Assert.Equal(new Code("elevator_broken"), stored.AttributeCode);
        Assert.Equal(new AccessibilityValue.Boolean(true), stored.Value);
    }
}
