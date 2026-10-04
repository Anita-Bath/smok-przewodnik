using MediatR;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;

namespace AB.SmokPrzewodnik.Application.Spatials.Commands;

public sealed record AddAccessibilityFactCommand(
    Guid PlaceId,
    string AttributeCode,
    bool Value) : IRequest<bool>;
