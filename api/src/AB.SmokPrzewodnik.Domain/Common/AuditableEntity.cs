namespace AB.SmokPrzewodnik.Domain.Common;

public abstract class AuditableEntity<TId> : Entity<TId>
    where TId : notnull
{
    protected AuditableEntity(
        TId id,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
        : base(id)
    {
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;

        if (updatedAt < CreatedAt)
        {
            throw new ArgumentException("The update timestamp cannot precede creation.", nameof(updatedAt));
        }

        UpdatedAt = updatedAt;
    }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public void MarkUpdated(DateTimeOffset updatedAt)
    {
        if (updatedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(updatedAt));
        }

        UpdatedAt = updatedAt;
    }
}
