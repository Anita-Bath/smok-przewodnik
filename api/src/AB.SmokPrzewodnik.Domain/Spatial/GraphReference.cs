using AB.SmokPrzewodnik.Domain.Common;
using AB.SmokPrzewodnik.Domain.Enums;

namespace AB.SmokPrzewodnik.Domain.Spatial;

public sealed record GraphReference
{
    public GraphReference(
        string routingProvider,
        string graphVersion,
        GraphElementType elementType,
        string externalElementId)
    {
        RoutingProvider = Guard.NotBlank(routingProvider, nameof(routingProvider));
        GraphVersion = Guard.NotBlank(graphVersion, nameof(graphVersion));
        ElementType = elementType;
        ExternalElementId = Guard.NotBlank(externalElementId, nameof(externalElementId));
    }

    public string RoutingProvider { get; }

    public string GraphVersion { get; }

    public GraphElementType ElementType { get; }

    public string ExternalElementId { get; }
}
