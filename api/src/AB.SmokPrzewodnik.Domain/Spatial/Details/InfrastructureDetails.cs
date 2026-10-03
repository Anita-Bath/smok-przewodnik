using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record InfrastructureDetails(
    Code InfrastructureCode,
    Code OperationalState,
    string? MaintenanceReference) : SpatialEntityDetails(EntityKind.Infrastructure);
