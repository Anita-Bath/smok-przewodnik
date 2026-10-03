using System.Collections.ObjectModel;

namespace AB.SmokPrzewodnik.Domain.Common;

public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];
    private readonly ReadOnlyCollection<IDomainEvent> _domainEventsView;

    protected AggregateRoot(TId id)
        : base(id)
    {
        _domainEventsView = _domainEvents.AsReadOnly();
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEventsView;

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
