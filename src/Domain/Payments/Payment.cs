using Domain.Subscriptions;
using Domain.Tenants;
using Domain.Users;

namespace Domain.Payments;

public sealed class Payment
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionBillingPeriod BillingPeriod { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public PaymentStatus Status { get; set; }
    public string Md5Hash { get; set; }
    public string QrData { get; set; }
    public string? TransactionHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public SubscriptionPlan? SubscriptionPlan { get; set; }
}
