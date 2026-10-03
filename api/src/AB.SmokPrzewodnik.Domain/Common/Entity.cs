using System.Runtime.CompilerServices;

namespace AB.SmokPrzewodnik.Domain.Common;

public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity()
    {
        Id = default!;
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; private set; }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return ReferenceEquals(left, right) || left is not null && left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    public bool Equals(Entity<TId>? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null || GetType() != other.GetType() || HasDefaultId || other.HasDefaultId)
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HasDefaultId
            ? RuntimeHelpers.GetHashCode(this)
            : HashCode.Combine(GetType(), Id);
    }

    private bool HasDefaultId => EqualityComparer<TId>.Default.Equals(Id, default!);
}
