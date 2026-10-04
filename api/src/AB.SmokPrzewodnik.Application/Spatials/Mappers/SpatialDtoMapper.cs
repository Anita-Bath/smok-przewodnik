using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.Spatial.Details;

namespace AB.SmokPrzewodnik.Application.Spatials.Mappers;

internal static class SpatialDtoMapper
{
    public static SpatialEntityListItemDto ToListItem(SpatialEntity entity) => new(
        entity.Id,
        entity.CityId,
        entity.Kind,
        entity.State,
        new ConfidenceSummaryDto(
            entity.Confidence.State,
            entity.Confidence.Score,
            entity.Confidence.EvidenceCount,
            entity.Confidence.EvaluatedAt),
        new GeometryDto(
            entity.Geometry.Kind,
            entity.Geometry.Coordinates
                .Select(coordinate => new GeoCoordinateDto(
                    coordinate.Latitude,
                    coordinate.Longitude))
                .ToArray()),
        ToTranslations(entity),
        entity.CreatedAt,
        entity.UpdatedAt);

    public static PlaceDto ToPlace(SpatialEntity entity, PlaceDetails details) => new(
        entity.Id,
        entity.CityId,
        entity.State,
        ToConfidence(entity),
        ToGeometry(entity),
        ToTranslations(entity),
        details.CategoryCode.Value,
        details.OpeningHours,
        details.Contact is null
            ? null
            : new ContactDto(details.Contact.Phone, details.Contact.Email),
        details.Website,
        entity.CreatedAt,
        entity.UpdatedAt);

    public static EventDto ToEvent(SpatialEntity entity, EventDetails details) => new(
        entity.Id,
        entity.CityId,
        entity.State,
        ToConfidence(entity),
        ToGeometry(entity),
        ToTranslations(entity),
        ToCategories(details),
        details.OrganizerEntityId,
        details.StartsAt,
        details.EndsAt,
        details.BookingUri,
        details.Capacity,
        entity.CreatedAt,
        entity.UpdatedAt);

    public static InfrastructureDto ToInfrastructure(SpatialEntity entity, InfrastructureDetails details) => new(
        entity.Id,
        entity.CityId,
        entity.Kind,
        entity.State,
        ToConfidence(entity),
        ToGeometry(entity),
        details.InfrastructureCode,
        details.OperationalState,
        details.MaintenanceReference,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    private static ConfidenceSummaryDto ToConfidence(SpatialEntity entity) => new(
        entity.Confidence.State,
        entity.Confidence.Score,
        entity.Confidence.EvidenceCount,
        entity.Confidence.EvaluatedAt);

    private static GeometryDto ToGeometry(SpatialEntity entity) => new(
        entity.Geometry.Kind,
        entity.Geometry.Coordinates
            .Select(coordinate => new GeoCoordinateDto(coordinate.Latitude, coordinate.Longitude))
            .ToArray());

    private static IReadOnlyList<TranslationDto> ToTranslations(SpatialEntity entity) =>
        entity.Translations
            .OrderBy(translation => translation.Locale, StringComparer.OrdinalIgnoreCase)
            .Select(translation => new TranslationDto(
                translation.Locale,
                translation.Name,
                translation.Description))
            .ToArray();

    private static IReadOnlyList<string> ToCategories(EventDetails details) =>
        details.CategoryCodes
            .Select(category => category.Value)
            .Order(StringComparer.Ordinal)
            .ToArray();
}
