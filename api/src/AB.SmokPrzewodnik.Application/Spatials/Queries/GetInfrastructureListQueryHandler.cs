using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Application.Spatials.Mappers;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed class GetInfrastructureListQueryHandler : IRequestHandler<GetInfrastructureListQuery, CursorPageResponse<InfrastructureDto>>
{
    private readonly ISpatialEntityRepository _repository;

    public GetInfrastructureListQueryHandler(ISpatialEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<CursorPageResponse<InfrastructureDto>> Handle(GetInfrastructureListQuery request, CancellationToken cancellationToken)
    {
        var criteria = new SpatialEntityCriteria(EntityKind.Infrastructure);

        var page = await _repository.FindAsync(
            criteria,
            request.Page,
            cancellationToken);

        return new CursorPageResponse<InfrastructureDto>(
            page.Items.Select(entity =>
            {

                if (entity is not { Kind: EntityKind.Infrastructure, Details: InfrastructureDetails details })
                {
                    throw new InvalidOperationException(
                        "The infrastructure repository returned a non-infrastructure entity.");
                }

                return SpatialDtoMapper.ToInfrastructure(entity, details);
            }).ToArray(),
            page.NextCursor);
    }
}
