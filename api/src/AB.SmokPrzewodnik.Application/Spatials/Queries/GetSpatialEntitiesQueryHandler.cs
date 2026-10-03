using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Mappers;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed class GetSpatialEntitiesQueryHandler
    : IRequestHandler<GetSpatialEntitiesQuery, CursorPageResponse<SpatialEntityListItemDto>>
{
    private readonly ISpatialEntityRepository _repository;

    public GetSpatialEntitiesQueryHandler(ISpatialEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<CursorPageResponse<SpatialEntityListItemDto>> Handle(
        GetSpatialEntitiesQuery request,
        CancellationToken cancellationToken)
    {
        var page = await _repository.FindAsync(
            new SpatialEntityCriteria(),
            request.Page,
            cancellationToken);

        return new CursorPageResponse<SpatialEntityListItemDto>(
            page.Items.Select(SpatialDtoMapper.ToListItem).ToArray(),
            page.NextCursor);
    }
}
