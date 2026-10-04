using AB.SmokPrzewodnik.Application.Routing.Dtos;
using AB.SmokPrzewodnik.Application.Routing.Mappers;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Routing;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Routing.Commands;

public sealed class PlanRouteCommandHandler(
    IRoutePlanner routePlanner,
    IRoutePlanRepository routePlans,
    ISpatialEntityRepository spatialEntities,
    TimeProvider timeProvider) : IRequestHandler<PlanRouteCommand, RoutePlanResponse>
{
    public async Task<RoutePlanResponse> Handle(
        PlanRouteCommand command,
        CancellationToken cancellationToken)
    {
        var originalRequest = ToDomainRequest(command);
        var providerRequest = new RoutePlanRequest(
            await ResolveAsync(originalRequest.Origin, cancellationToken),
            await ResolveAsync(originalRequest.Destination, cancellationToken),
            originalRequest.TravelModes,
            originalRequest.Constraints,
            originalRequest.RequestedProfiles,
            originalRequest.Locale,
            originalRequest.ClientDataVersions);

        var hardConstraints = originalRequest.Constraints
            .Where(pair => pair.Value == ConstraintLevel.MustAvoid)
            .Select(pair => pair.Key)
            .ToHashSet();
        var alternatives = (await routePlanner.PlanAsync(providerRequest, cancellationToken))
            .Where(alternative => !alternative.AccessibilitySummary.RelevantUnknowns.Overlaps(hardConstraints))
            .ToArray();

        if (alternatives.Length == 0)
        {
            throw new NoViableRouteException();
        }

        var now = timeProvider.GetUtcNow();
        var expiresAt = alternatives.Min(alternative => alternative.ExpiresAt);
        var plan = new RoutePlan(Guid.NewGuid(), command.AccountId, originalRequest, alternatives, now, expiresAt);
        await routePlans.AddAsync(plan, cancellationToken);

        return new RoutePlanResponse(
            plan.Id,
            plan.ExpiresAt,
            plan.Alternatives.Select(RouteDtoMapper.ToDto).ToArray());
    }

    private async Task<RouteEndpoint> ResolveAsync(RouteEndpoint endpoint, CancellationToken cancellationToken)
    {
        if (endpoint is RouteEndpoint.Coordinate)
        {
            return endpoint;
        }

        var entityId = ((RouteEndpoint.Entity)endpoint).EntityId;
        var entity = await spatialEntities.GetByIdAsync(entityId, cancellationToken)
            ?? throw new RouteEndpointNotFoundException(entityId);
        return new RouteEndpoint.Coordinate(entity.Geometry.Coordinates[0]);
    }

    private static RoutePlanRequest ToDomainRequest(PlanRouteCommand command) => new(
        ToDomainEndpoint(command.Origin),
        ToDomainEndpoint(command.Destination),
        command.TravelModes,
        command.Constraints.ToDictionary(pair => new Code(pair.Key), pair => pair.Value),
        command.RequestedProfiles.Select(profile => new Code(profile)),
        command.Locale,
        command.ClientDataVersions.ToDictionary(pair => new Code(pair.Key), pair => pair.Value));

    private static RouteEndpoint ToDomainEndpoint(RouteEndpointDto endpoint)
    {
        var hasCoordinate = endpoint.Latitude.HasValue && endpoint.Longitude.HasValue;
        var hasPartialCoordinate = endpoint.Latitude.HasValue != endpoint.Longitude.HasValue;
        if (hasPartialCoordinate || hasCoordinate == endpoint.EntityId.HasValue)
        {
            throw new ArgumentException("A route endpoint must contain either one coordinate or one entity identifier.");
        }

        return endpoint.EntityId is { } entityId
            ? new RouteEndpoint.Entity(entityId)
            : new RouteEndpoint.Coordinate(new GeoCoordinate(endpoint.Latitude!.Value, endpoint.Longitude!.Value));
    }
}
