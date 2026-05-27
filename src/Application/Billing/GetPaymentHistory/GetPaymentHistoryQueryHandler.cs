using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Billing.GetPaymentHistory;

internal sealed class GetPaymentHistoryQueryHandler(
    IApplicationDbContext context)
    : IQueryHandler<GetPaymentHistoryQuery, List<PaymentResponse>>
{
    public async Task<Result<List<PaymentResponse>>> Handle(
        GetPaymentHistoryQuery query,
        CancellationToken cancellationToken)
    {
#pragma warning disable IDE0031
        List<PaymentResponse> payments = await context.Payments
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponse
            {
                Id = p.Id,
                Plan = p.SubscriptionPlan != null ? p.SubscriptionPlan.Name : string.Empty,
                BillingPeriod = p.BillingPeriod.ToString(),
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync(cancellationToken);
#pragma warning restore IDE0031

        return payments;
    }
}
