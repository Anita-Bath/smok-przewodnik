using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed class GetPlaceQueryHandler : IRequestHandler<GetPlaceQuery, PlaceDto?>
{
    private readonly ISpatialEntityRepository _repository;

    public GetPlaceQueryHandler(ISpatialEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlaceDto?> Handle(GetPlaceQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.PlaceId, cancellationToken);
        return entity is { Kind: EntityKind.Place, Details: PlaceDetails details }
            ? SpatialDtoMapper.ToPlace(entity, details)
            : null;
    }
}
