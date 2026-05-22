using Application.Abstractions.Billing;

namespace Application.Billing.CheckPayment;

public sealed record CheckPaymentResponse
{
    public bool IsCompleted { get; init; }
    public TransactionCheckResult? Transaction { get; init; }
}
