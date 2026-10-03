using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record EventDto(
    Guid Id,
    Guid CityId,
    LifecycleState State,
    ConfidenceSummaryDto Confidence,
    GeometryDto Geometry,
    IReadOnlyList<TranslationDto> Translations,
    IReadOnlyList<string> CategoryCodes,
    Guid? OrganizerEntityId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    Uri? BookingUri,
    uint? Capacity,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record EventListItemDto(
    Guid Id,
    Guid CityId,
    LifecycleState State,
    ConfidenceSummaryDto Confidence,
    GeometryDto Geometry,
    IReadOnlyList<TranslationDto> Translations,
    IReadOnlyList<string> CategoryCodes,
    Guid? OrganizerEntityId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    Uri? BookingUri,
    uint? Capacity,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
