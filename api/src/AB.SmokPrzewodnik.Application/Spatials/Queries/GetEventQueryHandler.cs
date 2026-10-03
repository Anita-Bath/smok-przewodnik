using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial.Details;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed class GetEventQueryHandler : IRequestHandler<GetEventQuery, EventDto?>
{
    private readonly ISpatialEntityRepository _repository;

    public GetEventQueryHandler(ISpatialEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDto?> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.EventId, cancellationToken);
        return entity is { Kind: EntityKind.Event, Details: EventDetails details }
            ? SpatialDtoMapper.ToEvent(entity, details)
            : null;
    }
}
