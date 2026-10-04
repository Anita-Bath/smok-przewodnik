using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Domain.Enums;
using AB.SmokPrzewodnik.Domain.Spatial;
using AB.SmokPrzewodnik.Domain.ValueObjects;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace AB.SmokPrzewodnik.Application.Spatials.Commands;

public sealed class AddAccessibilityFactCommandHandler : IRequestHandler<AddAccessibilityFactCommand, bool>
{
    private readonly ISpatialEntityRepository _repository;
    private readonly TimeProvider _timeProvider;

    public AddAccessibilityFactCommandHandler(ISpatialEntityRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<bool> Handle(AddAccessibilityFactCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.PlaceId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        var now = _timeProvider.GetUtcNow();

        // Create the accessibility fact
        var fact = new AccessibilityFact(
            Guid.NewGuid(),
            new AccessibilityFactTarget.SpatialEntity(entity.Id),
            new Code(request.AttributeCode),
            new AccessibilityValue.Boolean(request.Value),
            new EvidenceReference.Observation(Guid.NewGuid()), // Treating user input as an observation
            now,
            now,
            null,
            0.5m // Default confidence for crowd-sourced user input
        );

        entity.AddAccessibilityFact(fact, now);
        await _repository.UpdateAsync(entity, cancellationToken);

        return true;
    }
}
