using Domain.Subscriptions;
using SharedKernel;

namespace Domain.Tenants;

public sealed class Tenant : Entity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }
    public int SeatCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Subscription? Subscription { get; set; }
}
