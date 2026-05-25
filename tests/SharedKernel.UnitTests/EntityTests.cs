using SharedKernel;

namespace SharedKernel.UnitTests;

public sealed class EntityTests
{
    private sealed class TestEntity : Entity
    {
    }

    private sealed record TestDomainEvent : IDomainEvent;

    [Fact]
    public void Raise_ShouldAddDomainEvent()
    {
        TestEntity entity = new();
        IDomainEvent domainEvent = new TestDomainEvent();

        entity.Raise(domainEvent);

        entity.DomainEvents.Count.ShouldBe(1);
        entity.DomainEvents[0].ShouldBe(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyList()
    {
        TestEntity entity = new();
        entity.Raise(new TestDomainEvent());
        entity.Raise(new TestDomainEvent());

        entity.ClearDomainEvents();

        entity.DomainEvents.Count.ShouldBe(0);
    }

    [Fact]
    public void DomainEvents_ShouldReturnCopy()
    {
        TestEntity entity = new();
        entity.Raise(new TestDomainEvent());

        List<IDomainEvent> events = entity.DomainEvents;

        events.ShouldNotBeSameAs(entity.DomainEvents);
    }
}
