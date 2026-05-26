using Domain.Users;
using SharedKernel;

namespace Domain.UnitTests.Users;

public sealed class UserDomainEventTests
{
    [Fact]
    public void ShouldImplementIDomainEventSource()
    {
        typeof(IDomainEventSource).IsAssignableFrom(typeof(User)).ShouldBeTrue();
    }

    [Fact]
    public void Raise_ShouldAddDomainEvent()
    {
        User user = new();
        IDomainEvent domainEvent = new UserRegisteredDomainEvent(Guid.NewGuid(), "test@example.com", "token");

        user.Raise(domainEvent);

        user.DomainEvents.Count.ShouldBe(1);
        user.DomainEvents[0].ShouldBe(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        User user = new();
        user.Raise(new UserRegisteredDomainEvent(Guid.NewGuid(), "test@example.com", "token"));
        user.Raise(new UserRegisteredDomainEvent(Guid.NewGuid(), "test2@example.com", "token"));

        user.ClearDomainEvents();

        user.DomainEvents.Count.ShouldBe(0);
    }
}
