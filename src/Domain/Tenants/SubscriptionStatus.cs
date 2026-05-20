namespace Domain.Tenants;

public enum SubscriptionStatus
{
    Active = 0,
    Trialing = 1,
    PastDue = 2,
    Canceled = 3,
    Expired = 4
}
