using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Confidence;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed class SpatialEntity : AuditableEntity<Guid>
{
    public Guid CityId { get; set; }
    public EntityKind Kind { get; set; }
    public LifecycleState State { get; set; }
    public ConfidenceAssessment Confidence { get; set; }

    public SpatialEntity(Guid id, Guid cityId, EntityKind kind, LifecycleState state, ConfidenceAssessment confidence) : base(id)
    {
        CityId = cityId;
        Kind = kind;
        State = state;
        Confidence = confidence;
    }
}
