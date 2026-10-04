using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record PlaceDto(
    Guid Id,
    Guid CityId,
    LifecycleState State,
    ConfidenceSummaryDto Confidence,
    GeometryDto Geometry,
    IReadOnlyList<TranslationDto> Translations,
    string CategoryCode,
    string? OpeningHours,
    ContactDto? Contact,
    Uri? Website,
    IReadOnlyList<AccessibilityFactDto> AccessibilityFacts,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ContactDto(
    string? Phone,
    string? Email);

public sealed record AccessibilityFactDto(
    Guid Id,
    string AttributeCode,
    AccessibilityValueDto Value,
    DateTimeOffset ObservedAt,
    DateTimeOffset? ValidFrom,
    DateTimeOffset? ValidUntil,
    decimal ConfidenceWeight);

public sealed record AccessibilityValueDto(
    string Kind,
    bool? BooleanValue = null,
    decimal? NumberValue = null,
    string? UnitCode = null,
    string? CodeValue = null,
    string? TextValue = null);
