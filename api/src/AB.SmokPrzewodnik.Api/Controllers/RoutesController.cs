using System.Security.Claims;
using AB.SmokPrzewodnik.Application.Routing.Commands;
using AB.SmokPrzewodnik.Application.Routing.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AB.SmokPrzewodnik.Api.Controllers;

[ApiController]
[Route("routes")]
public sealed class RoutesController(IMediator mediator) : ControllerBase
{
    [HttpPost("plan")]
    [AllowAnonymous]
    [ProducesResponseType<RoutePlanResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<RoutePlanResponse>> PlanRoute(
        [FromBody] PlanRouteRequestDto request,
        CancellationToken cancellationToken)
    {
        var accountId = User.Identity?.IsAuthenticated == true &&
                        Guid.TryParse(User.FindFirstValue("sub"), out var parsedAccountId)
            ? parsedAccountId
            : (Guid?)null;

        var response = await mediator.Send(
            PlanRouteCommand.FromRequest(request, accountId),
            cancellationToken);

        return Ok(response);
    }
}
