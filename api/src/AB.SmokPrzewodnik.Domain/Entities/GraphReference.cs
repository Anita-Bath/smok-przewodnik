using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Entities;

public sealed record GraphReference(string RoutingProvider,
    string GraphVersion,
    GraphElementType ElementType,
    string ExternalElementId);
