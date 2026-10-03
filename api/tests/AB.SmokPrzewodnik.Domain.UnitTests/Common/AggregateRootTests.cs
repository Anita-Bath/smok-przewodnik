using AB.SmokPrzewodnik.Domain.Common;
using Xunit;

namespace AB.SmokPrzewodnik.Domain.UnitTests.Common;

public sealed class AggregateRootTests
{
    [Fact]
    public void RaiseDomainEvent_AddsEventInInsertionOrder()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var first = new TestDomainEvent("first");
        var second = new TestDomainEvent("second");

        aggregate.Record(first);
        aggregate.Record(second);

        Assert.Equal([first, second], aggregate.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllRaisedEvents()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Record(new TestDomainEvent("event"));

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void DomainEvents_CannotBeMutatedThroughExposedCollection()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var exposedEvents = Assert.IsAssignableFrom<ICollection<IDomainEvent>>(aggregate.DomainEvents);

        Assert.True(exposedEvents.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => exposedEvents.Add(new TestDomainEvent("event")));
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Record(IDomainEvent domainEvent)
        {
            RaiseDomainEvent(domainEvent);
        }
    }

    private sealed record TestDomainEvent(string Name) : IDomainEvent;
}
