using Domain.Tenants;

namespace Application.Admin.GetAllTenants;

public sealed record TenantAdminResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Identifier { get; init; }
    public string? SubscriptionPlanName { get; init; }
    public SubscriptionStatus? SubscriptionStatus { get; init; }
    public int? MaxUsersOverride { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}
