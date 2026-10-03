using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Planning;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Spatial;

public sealed class EntityNavigationTests
{
    [Theory]
    [InlineData(typeof(SpatialEntity), "City", typeof(City))]
    [InlineData(typeof(EntityTranslation), "Entity", typeof(SpatialEntity))]
    [InlineData(typeof(EntitySourceLink), "Entity", typeof(SpatialEntity))]
    [InlineData(typeof(EntitySourceLink), "Source", typeof(DataSource))]
    [InlineData(typeof(SourceAssertion), "Source", typeof(DataSource))]
    [InlineData(typeof(AccessibilityFact), "SpatialEntity", typeof(SpatialEntity))]
    [InlineData(typeof(AccessibilityFact), "SourceAssertion", typeof(SourceAssertion))]
    [InlineData(typeof(ItineraryItem), "Entity", typeof(SpatialEntity))]
    [InlineData(typeof(FeedItem), "City", typeof(City))]
    [InlineData(typeof(FeedItem), "Source", typeof(DataSource))]
    [InlineData(typeof(SuggestionProjection), "SuggestedEntity", typeof(SpatialEntity))]
    [InlineData(typeof(EventDetails), "Organizer", typeof(SpatialEntity))]
    public void ReferenceNavigations_HaveExpectedTypesAndNonPublicSetters(
        Type declaringType,
        string propertyName,
        Type navigationType)
    {
        var property = declaringType.GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(navigationType, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
        Assert.NotNull(property.SetMethod);
        Assert.False(property.SetMethod.IsPublic);
    }

    [Theory]
    [InlineData(typeof(City), "SpatialEntities", typeof(SpatialEntity))]
    [InlineData(typeof(DataSource), "SourceLinks", typeof(EntitySourceLink))]
    [InlineData(typeof(DataSource), "SourceAssertions", typeof(SourceAssertion))]
    [InlineData(typeof(SpatialEntity), "AccessibilityFacts", typeof(AccessibilityFact))]
    [InlineData(typeof(SourceAssertion), "AccessibilityFacts", typeof(AccessibilityFact))]
    [InlineData(typeof(City), "FeedItems", typeof(FeedItem))]
    [InlineData(typeof(DataSource), "FeedItems", typeof(FeedItem))]
    [InlineData(typeof(SpatialEntity), "ItineraryItems", typeof(ItineraryItem))]
    [InlineData(typeof(SpatialEntity), "FeedItems", typeof(FeedItem))]
    [InlineData(typeof(SpatialEntity), "Suggestions", typeof(SuggestionProjection))]
    [InlineData(typeof(FeedItem), "LinkedEntities", typeof(SpatialEntity))]
    public void CollectionNavigations_AreReadOnlyCollections(
        Type declaringType,
        string propertyName,
        Type elementType)
    {
        var property = declaringType.GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.Equal(typeof(IReadOnlyCollection<>).MakeGenericType(elementType), property.PropertyType);
        Assert.Null(property.SetMethod);
    }

    [Fact]
    public void AccessibilityFact_ExposesRelationalKeysForEntityAndAssertionReferences()
    {
        var entityId = Guid.NewGuid();
        var assertionId = Guid.NewGuid();
        var fact = new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.SpatialEntity(entityId),
            new Code("step_free_access"),
            new AccessibilityValue.Boolean(true),
            new EvidenceReference.SourceAssertion(assertionId),
            DateTimeOffset.UtcNow,
            null,
            null,
            0.8m);

        Assert.Equal(entityId, ReadProperty<Guid?>(fact, "SpatialEntityId"));
        Assert.Equal(assertionId, ReadProperty<Guid?>(fact, "SourceAssertionId"));
    }

    [Fact]
    public void AccessibilityFact_LeavesRelationalKeysNullForGraphAndObservationReferences()
    {
        var fact = new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.GraphElement(
                new GraphReference("valhalla", "v1", GraphElementType.Edge, "42")),
            new Code("surface"),
            new AccessibilityValue.Code(new Code("cobblestone")),
            new EvidenceReference.Observation(Guid.NewGuid()),
            DateTimeOffset.UtcNow,
            null,
            null,
            0.5m);

        Assert.Null(ReadProperty<Guid?>(fact, "SpatialEntityId"));
        Assert.Null(ReadProperty<Guid?>(fact, "SourceAssertionId"));
    }

    private static T ReadProperty<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return (T)property.GetValue(instance)!;
    }
}
