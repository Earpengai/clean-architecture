using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Domain.Tenants;

public sealed class Role : IdentityRole<Guid>, IDomainEventSource
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<RolePermission> Permissions { get; set; } = [];
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
