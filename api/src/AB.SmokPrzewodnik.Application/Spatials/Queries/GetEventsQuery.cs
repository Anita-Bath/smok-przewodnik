using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed record GetEventsQuery(
    BoundingBox? BoundingBox,
    DateTimeOffset From,
    DateTimeOffset? To,
    IReadOnlyCollection<string> Categories,
    CursorPageRequest Page,
    bool FromIsDynamic = false) : IRequest<CursorPageResponse<EventListItemDto>>;
