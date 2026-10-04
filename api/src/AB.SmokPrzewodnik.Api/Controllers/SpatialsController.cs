using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace AB.SmokPrzewodnik.Api.Controllers;

[ApiController]
[Route("/spatials")]
public sealed class SpatialsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly TimeProvider _timeProvider;

    public SpatialsController(IMediator mediator, TimeProvider timeProvider)
    {
        _mediator = mediator;
        _timeProvider = timeProvider;
    }

    [HttpGet("entities")]
    [AllowAnonymous]
    [ProducesResponseType<CursorPageResponse<SpatialEntityListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CursorPageResponse<SpatialEntityListItemDto>>> FindAsync(
        CancellationToken cancellationToken,
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = CursorPageRequest.DefaultLimit)
    {
        CursorPageRequest page;
        try
        {
            page = new CursorPageRequest(cursor, limit);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination limit.",
                detail: exception.Message);
        }

        var response = await _mediator.Send(
            new GetSpatialEntitiesQuery(page),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("/places/{placeId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<PlaceDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlaceDto>> GetPlaceAsync(
        Guid placeId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new GetPlaceQuery(placeId),
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("/events/{eventId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<EventDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDto>> GetEventAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(
            new GetEventQuery(eventId),
            cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("/events")]
    [AllowAnonymous]
    [ProducesResponseType<CursorPageResponse<EventDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CursorPageResponse<EventDto>>> FindEventsAsync(
        CancellationToken cancellationToken,
        [FromQuery] string? bbox = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        [FromQuery] string? categories = null,
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = CursorPageRequest.DefaultLimit)
    {
        if (!TryParseBoundingBox(bbox, out var boundingBox, out var boundsError))
        {
            return InvalidQuery("Invalid bounding box.", boundsError);
        }

        var categoryValues = ParseCategories(categories);
        if (categories is not null && categoryValues.Count == 0)
        {
            return InvalidQuery(
                "Invalid event categories.",
                "At least one non-empty category is required when categories are supplied.");
        }

        var effectiveFrom = from ?? _timeProvider.GetUtcNow();
        if (to < effectiveFrom)
        {
            return InvalidQuery(
                "Invalid event time range.",
                "The end of the time range cannot precede its start.");
        }

        CursorPageRequest page;
        try
        {
            page = new CursorPageRequest(cursor, limit);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return InvalidQuery("Invalid pagination limit.", exception.Message);
        }

        var response = await _mediator.Send(
            new GetEventsQuery(
                boundingBox,
                effectiveFrom,
                to,
                categoryValues,
                page,
                from is null),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("/infrastructure")]
    [AllowAnonymous]
    [ProducesResponseType<CursorPageResponse<InfrastructureDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CursorPageResponse<InfrastructureDto>>> FindInfrastructureAsync(
        CancellationToken cancellationToken,
        [FromQuery] string? cursor = null,
        [FromQuery] int limit = CursorPageRequest.DefaultLimit)
    {
        CursorPageRequest page;
        try
        {
            page = new CursorPageRequest(cursor, limit);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid pagination limit.",
                detail: exception.Message);
        }

        var response = await _mediator.Send(
            new GetInfrastructureListQuery(page),
            cancellationToken);

        return Ok(response);
    }

    private ObjectResult InvalidQuery(string title, string detail) =>
        Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: title,
            detail: detail);

    private static IReadOnlyCollection<string> ParseCategories(string? categories) =>
        categories is null
            ? []
            : categories
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(category => category.ToLowerInvariant())
                .Distinct(StringComparer.Ordinal)
                .ToArray();

    private static bool TryParseBoundingBox(
        string? value,
        out BoundingBox? boundingBox,
        out string error)
    {
        boundingBox = null;
        error = string.Empty;
        if (value is null)
        {
            return true;
        }

        var parts = value.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 4 || parts.Any(part => !decimal.TryParse(
                part,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out _)))
        {
            error = "The bbox parameter must contain four comma-separated numbers.";
            return false;
        }

        var values = parts
            .Select(part => decimal.Parse(part, NumberStyles.Float, CultureInfo.InvariantCulture))
            .ToArray();

        try
        {
            boundingBox = new BoundingBox(values[0], values[1], values[2], values[3]);
            return true;
        }
        catch (ArgumentException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    [HttpPost("/places/{placeId:guid}/accessibility")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAccessibilityFact(
        Guid placeId,
        [FromBody] AddAccessibilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AB.SmokPrzewodnik.Application.Spatials.Commands.AddAccessibilityFactCommand(
                placeId, request.AttributeCode, request.Value),
            cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return Ok();
    }
}

public sealed record AddAccessibilityRequest(string AttributeCode, bool Value);
