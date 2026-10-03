namespace AB.SmokPrzewodnik.Domain.Planning;

public sealed record ItineraryItem
{
    public ItineraryItem(
        Guid id,
        int position,
        Guid entityId,
        DateTimeOffset? plannedArrival,
        DateTimeOffset? plannedDeparture,
        string? note)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The item identifier cannot be empty.", nameof(id));
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("The entity identifier cannot be empty.", nameof(entityId));
        }

        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        if (plannedArrival.HasValue && plannedDeparture.HasValue && plannedDeparture < plannedArrival)
        {
            throw new ArgumentException("Departure cannot precede arrival.", nameof(plannedDeparture));
        }

        Id = id;
        Position = position;
        EntityId = entityId;
        PlannedArrival = plannedArrival;
        PlannedDeparture = plannedDeparture;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public Guid Id { get; }

    public int Position { get; }

    public Guid EntityId { get; }

    public DateTimeOffset? PlannedArrival { get; }

    public DateTimeOffset? PlannedDeparture { get; }

    public string? Note { get; }

    internal ItineraryItem AtPosition(int position) =>
        new(Id, position, EntityId, PlannedArrival, PlannedDeparture, Note);
}
