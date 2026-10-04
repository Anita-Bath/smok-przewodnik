using System.Security.Claims;
using AB.SmokPrzewodnik.Application.Navigation.Commands;
using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using AB.SmokPrzewodnik.Application.Navigation.Queries;
using AB.SmokPrzewodnik.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("navigation")]
public sealed class NavigationController(IMediator mediator) : ControllerBase
{

    [HttpGet]
    [Route("sessions/{sessionId:guid}/events")]
    [Authorize(Policy = AppConsts.NavigationAuthPolicyName)]
    public async Task<ActionResult<NavigationEventsResponse>> GetSessionEvents(
        [FromRoute] Guid sessionId,
        [FromQuery] long afterSequence = 0,
        CancellationToken cancellationToken = default)
    {
        return Ok(await mediator.Send(
            new GetNavigationEventsQuery(sessionId, afterSequence), cancellationToken));
    }

    [HttpPost]
    [Route("sessions")]
    [AllowAnonymous]
    [ProducesResponseType<StartNavigationSessionResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<StartNavigationSessionResponse>> StartSession(
        [FromBody] StartNavigationSessionRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = User.Identity?.IsAuthenticated == true &&
                        Guid.TryParse(User.FindFirstValue("sub"), out var parsedAccountId)
            ? parsedAccountId
            : (Guid?)null;
        var response = await mediator.Send(new StartNavigationSessionCommand(
            request.RoutePlanId,
            request.RouteAlternativeId,
            accountId), cancellationToken);
        return Created($"/v1/navigation/sessions/{response.SessionId}", response);
    }

    [HttpPost]
    [Route("sessions/{sessionId:guid}/progress")]
    [Authorize(Policy = AppConsts.NavigationAuthPolicyName)]
    public async Task<ActionResult<NavigationProgressResponse>> UpdateSessionProgress(
        [FromRoute] Guid sessionId,
        [FromBody] UpdateNavigationProgressRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(
            new UpdateNavigationProgressCommand(sessionId, request), cancellationToken));
    }

    [HttpPost]
    [Route("sessions/{sessionId:guid}/reroute")]
    [Authorize(Policy = AppConsts.NavigationAuthPolicyName)]
    public async Task<ActionResult<RerouteNavigationSessionResponse>> RerouteSession(
        [FromRoute] Guid sessionId,
        [FromBody] RerouteNavigationSessionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new RerouteNavigationSessionCommand(
            sessionId, request.RoutePlanId, request.RouteAlternativeId), cancellationToken));
    }

    [HttpDelete]
    [Route("sessions/{sessionId:guid}")]
    [Authorize(Policy = AppConsts.NavigationAuthPolicyName)]
    public async Task<IActionResult> DeleteSession(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteNavigationSessionCommand(sessionId), cancellationToken);
        return NoContent();
    }
}
