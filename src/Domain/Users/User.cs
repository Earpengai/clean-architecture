using Domain.Tenants;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Domain.Users;

public sealed class User : IdentityUser<Guid>, IDomainEventSource
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsSystemAdministrator { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Membership> Memberships { get; set; } = [];

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
