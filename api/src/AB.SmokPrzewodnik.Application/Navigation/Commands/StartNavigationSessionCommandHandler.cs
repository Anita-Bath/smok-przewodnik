using AB.SmokPrzewodnik.Application.Auth.NavigationToken;
using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Routing;
using AB.SmokPrzewodnik.Application.Routing.Mappers;
using AB.SmokPrzewodnik.Domain.Navigation;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed class StartNavigationSessionCommandHandler(
    IRoutePlanRepository routePlans,
    INavigationSessionRepository sessions,
    INavigationTokenService tokenService,
    TimeProvider timeProvider) : IRequestHandler<StartNavigationSessionCommand, StartNavigationSessionResponse>
{
    public async Task<StartNavigationSessionResponse> Handle(
        StartNavigationSessionCommand command,
        CancellationToken cancellationToken)
    {
        var plan = await routePlans.GetByIdAsync(command.RoutePlanId, cancellationToken)
            ?? throw new RoutePlanNotFoundException(command.RoutePlanId);
        var now = timeProvider.GetUtcNow();
        if (plan.IsExpired(now))
        {
            throw new RoutePlanExpiredException(plan.Id);
        }

        if (plan.AccountId.HasValue && plan.AccountId != command.AccountId)
        {
            throw new RoutePlanAccessDeniedException(plan.Id);
        }

        var route = plan.FindAlternative(command.RouteAlternativeId)
            ?? throw new RouteAlternativeNotFoundException(command.RouteAlternativeId);
        var token = tokenService.IssueToken();

        now = timeProvider.GetUtcNow();
        if (plan.IsExpired(now))
        {
            throw new RoutePlanExpiredException(plan.Id);
        }

        var session = new NavigationSession(
            Guid.NewGuid(),
            token.Hash,
            command.AccountId,
            route,
            plan.Request,
            now,
            now.AddHours(1));
        await sessions.AddAsync(session, cancellationToken);

        return new StartNavigationSessionResponse(
            session.Id,
            token.Value,
            session.ExpiresAt,
            RouteDtoMapper.ToDto(session.ActiveRoute));
    }
}
