using SharedKernel;

namespace Domain.Tenants;

public sealed class Tenant : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }
    public SubscriptionStatus SubscriptionStatus { get; set; }
    public int SeatCount { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
