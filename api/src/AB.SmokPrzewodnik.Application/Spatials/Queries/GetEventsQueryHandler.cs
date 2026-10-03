using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed class GetEventsQueryHandler
    : IRequestHandler<GetEventsQuery, CursorPageResponse<EventListItemDto>>
{
    private readonly ISpatialEntityRepository _repository;

    public GetEventsQueryHandler(ISpatialEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<CursorPageResponse<EventListItemDto>> Handle(
        GetEventsQuery request,
        CancellationToken cancellationToken)
    {
        var criteria = new SpatialEntityCriteria(
            EntityKind.Event,
            request.BoundingBox,
            request.From,
            request.To,
            request.Categories,
            request.FromIsDynamic);
        var page = await _repository.FindAsync(criteria, request.Page, cancellationToken);
        var items = page.Items.Select(entity =>
        {
            if (entity is not { Kind: EntityKind.Event, Details: EventDetails details })
            {
                throw new InvalidOperationException("The event repository returned a non-event entity.");
            }

            return SpatialDtoMapper.ToEventListItem(entity, details);
        }).ToArray();

        return new CursorPageResponse<EventListItemDto>(items, page.NextCursor);
    }
}
