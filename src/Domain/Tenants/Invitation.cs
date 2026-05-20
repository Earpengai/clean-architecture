using SharedKernel;

namespace Domain.Tenants;

public sealed class Invitation : Entity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public string Email { get; set; }
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public string Token { get; set; }
    public DateTime TokenExpiry { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Expired = 2,
    Canceled = 3
}
