using Domain.Users;
using SharedKernel;

namespace Domain.Tenants;

public sealed class Membership : Entity
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public MembershipRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}
