using AB.SmokPrzewodnik.Application.Routing.Dtos;
using AB.SmokPrzewodnik.Domain.Enums;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Routing.Commands;

public sealed record PlanRouteCommand(
    RouteEndpointDto Origin,
    RouteEndpointDto Destination,
    IReadOnlyCollection<TravelMode> TravelModes,
    IReadOnlyDictionary<string, ConstraintLevel> Constraints,
    IReadOnlyCollection<string> RequestedProfiles,
    string Locale,
    IReadOnlyDictionary<string, string> ClientDataVersions,
    Guid? AccountId) : IRequest<RoutePlanResponse>
{
    public static PlanRouteCommand FromRequest(PlanRouteRequestDto request, Guid? accountId) => new(
        request.Origin,
        request.Destination,
        request.TravelModes,
        request.Constraints,
        request.RequestedProfiles,
        request.Locale,
        request.ClientDataVersions,
        accountId);
}
