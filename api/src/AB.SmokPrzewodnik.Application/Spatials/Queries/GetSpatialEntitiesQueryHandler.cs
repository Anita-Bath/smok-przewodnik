using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Spatial;
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
            page.Items.Select(Map).ToArray(),
            page.NextCursor);
    }

    private static SpatialEntityListItemDto Map(SpatialEntity entity) => new(
        entity.Id,
        entity.CityId,
        entity.Kind,
        entity.State,
        new ConfidenceSummaryDto(
            entity.Confidence.State,
            entity.Confidence.Score,
            entity.Confidence.EvidenceCount,
            entity.Confidence.EvaluatedAt),
        new GeometryDto(
            entity.Geometry.Kind,
            entity.Geometry.Coordinates
                .Select(coordinate => new GeoCoordinateDto(
                    coordinate.Latitude,
                    coordinate.Longitude))
                .ToArray()),
        entity.CreatedAt,
        entity.UpdatedAt);
}
