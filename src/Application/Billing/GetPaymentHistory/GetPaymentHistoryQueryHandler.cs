using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Billing.GetPaymentHistory;

internal sealed class GetPaymentHistoryQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetPaymentHistoryQuery, List<PaymentResponse>>
{
    public async Task<Result<List<PaymentResponse>>> Handle(
        GetPaymentHistoryQuery query,
        CancellationToken cancellationToken)
    {
        Guid tenantId = userContext.TenantId ?? Guid.Empty;

        List<PaymentResponse> payments = await context.Payments
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponse
            {
                Id = p.Id,
                Plan = p.Plan.ToString(),
                BillingPeriod = p.BillingPeriod.ToString(),
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status.ToString(),
                CreatedAt = p.CreatedAt,
                CompletedAt = p.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return payments;
    }
}
