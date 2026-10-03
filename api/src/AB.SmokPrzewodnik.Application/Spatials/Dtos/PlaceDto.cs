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
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record ContactDto(
    string? Phone,
    string? Email);
