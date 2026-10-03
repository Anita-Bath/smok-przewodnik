using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record SpatialEntityListItemDto(
    Guid Id,
    Guid CityId,
    EntityKind Kind,
    LifecycleState State,
    ConfidenceSummaryDto Confidence,
    GeometryDto Geometry,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
