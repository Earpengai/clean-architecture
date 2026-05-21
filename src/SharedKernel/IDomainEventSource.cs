namespace SharedKernel;

public interface IDomainEventSource
{
    List<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();

    void Raise(IDomainEvent domainEvent);
}
