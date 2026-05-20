using SharedKernel;

namespace Domain.Tenants;

public sealed class Role : Entity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<RolePermission> Permissions { get; set; } = [];
    public ICollection<Membership> Memberships { get; set; } = [];
}
