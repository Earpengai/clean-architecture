namespace Application.Billing.GetPaymentHistory;

public sealed record PaymentResponse
{
    public Guid Id { get; init; }
    public string Plan { get; init; }
    public string BillingPeriod { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}
