using Finbuckle.MultiTenant;

namespace Infrastructure.Multitenancy;

public sealed class AppTenantInfo : TenantInfo
{
    public string SubscriptionPlan { get; set; } = string.Empty;
    public string SubscriptionStatus { get; set; } = string.Empty;
    public int? MaxUsersOverride { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int CurrentUserCount { get; set; }
}
