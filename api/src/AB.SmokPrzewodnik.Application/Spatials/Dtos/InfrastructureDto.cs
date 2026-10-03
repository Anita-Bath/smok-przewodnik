using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Application.Spatials.Dtos;

public sealed record InfrastructureDto(
    Guid Id,
    Guid CityId,
    EntityKind Kind,
    LifecycleState State,
    ConfidenceSummaryDto Confidence,
    GeometryDto Geometry,
    Code InfrastructureCode,
    Code OperationalState,
    string? MaintenanceReference,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
