using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Application.Routing.Mappers;
using AB.SmokPrzewodnik.Application.Navigation.Mappers;
using AB.SmokPrzewodnik.Domain.Navigation;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed class RerouteNavigationSessionCommandHandler(
    INavigationSessionRepository sessions,
    IRoutePlanRepository routePlans,
    TimeProvider timeProvider,
    INavigationEventPublisher? eventPublisher = null) : IRequestHandler<RerouteNavigationSessionCommand, RerouteNavigationSessionResponse>
{
    public async Task<RerouteNavigationSessionResponse> Handle(
        RerouteNavigationSessionCommand command,
        CancellationToken cancellationToken)
    {
        var session = await sessions.GetByIdAsync(command.SessionId, cancellationToken)
            ?? throw new NavigationSessionNotFoundException(command.SessionId);
        var now = timeProvider.GetUtcNow();
        if (session.IsExpired(now))
        {
            throw new NavigationSessionExpiredException(session.Id);
        }

        var plan = await routePlans.GetByIdAsync(command.RoutePlanId, cancellationToken)
            ?? throw new RoutePlanNotFoundException(command.RoutePlanId);
        if (plan.IsExpired(now))
        {
            throw new RoutePlanExpiredException(plan.Id);
        }

        if (plan.AccountId.HasValue && plan.AccountId != session.AccountId)
        {
            throw new RoutePlanAccessDeniedException(plan.Id);
        }

        var route = plan.FindAlternative(command.RouteAlternativeId)
            ?? throw new RouteAlternativeNotFoundException(command.RouteAlternativeId);
        if (session.ReplaceRoute(route, plan.Request, now))
        {
            var rerouteEvent = session.AppendEvent(
                NavigationEventType.Reroute,
                NavigationUrgency.Advisory,
                null,
                null,
                null,
                route.DistanceMetres,
                new Dictionary<string, string>(),
                [new Code("audio"), new Code("haptic"), new Code("visual")],
                now);
            await sessions.UpdateAsync(session, [rerouteEvent], cancellationToken);
            try
            {
                await (eventPublisher ?? new NullNavigationEventPublisher()).PublishAsync(
                    session.Id,
                    NavigationEventMapper.ToDto(rerouteEvent),
                    cancellationToken);
            }
            catch
            {
                // The persisted event is recovered through HTTP if realtime delivery fails.
            }
        }

        return new RerouteNavigationSessionResponse(
            RouteDtoMapper.ToDto(session.ActiveRoute),
            session.NextEventSequence - 1);
    }
}
