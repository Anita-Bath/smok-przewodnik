using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.ValueObjects;

namespace AB.SmokPrzewodnik.Domain.Spatial.Details;

public sealed record InfrastructureDetails : SpatialEntityDetails
{
    private InfrastructureDetails() : base(EntityKind.Infrastructure)
    {
    }

    public InfrastructureDetails(
        Code infrastructureCode,
        Code operationalState,
        string? maintenanceReference)
        : base(EntityKind.Infrastructure)
    {
        InfrastructureCode = infrastructureCode;
        OperationalState = operationalState;
        MaintenanceReference = maintenanceReference;
    }

    public Code InfrastructureCode { get; private set; }
    public Code OperationalState { get; private set; }
    public string? MaintenanceReference { get; private set; }
}
