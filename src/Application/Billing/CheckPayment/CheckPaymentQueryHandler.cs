using Application.Abstractions.Authentication;
using Application.Abstractions.Billing;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Payments;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Billing.CheckPayment;

internal sealed class CheckPaymentQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext,
    IBakongService bakongService)
    : IQueryHandler<CheckPaymentQuery, CheckPaymentResponse>
{
    public async Task<Result<CheckPaymentResponse>> Handle(
        CheckPaymentQuery query,
        CancellationToken cancellationToken)
    {
        Guid tenantId = userContext.TenantId ?? Guid.Empty;

        Payment? payment = await context.Payments
            .FirstOrDefaultAsync(p => p.Md5Hash == query.Md5 && p.TenantId == tenantId, cancellationToken);

        if (payment is null)
        {
            return Result.Failure<CheckPaymentResponse>(PaymentErrors.NotFoundByMd5(query.Md5));
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            return new CheckPaymentResponse { IsCompleted = true };
        }

        if (payment.Status == PaymentStatus.Failed)
        {
            return new CheckPaymentResponse { IsCompleted = false };
        }

        Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            payment.Status = PaymentStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            return Result.Failure<CheckPaymentResponse>(TenantErrors.NotFound(tenantId));
        }

        TransactionCheckResult? result = await bakongService.CheckTransactionAsync(query.Md5, cancellationToken);

        if (result is null)
        {
            return new CheckPaymentResponse { IsCompleted = false };
        }

        payment.Status = PaymentStatus.Completed;
        payment.TransactionHash = result.Hash;
        payment.CompletedAt = DateTime.UtcNow;

        tenant.SubscriptionPlan = payment.Plan;
        tenant.BillingPeriod = payment.BillingPeriod;
        tenant.SubscriptionStatus = SubscriptionStatus.Active;
        tenant.SubscriptionExpiresAt = payment.BillingPeriod switch
        {
            SubscriptionBillingPeriod.Monthly => DateTime.UtcNow.AddMonths(1),
            SubscriptionBillingPeriod.Yearly => DateTime.UtcNow.AddYears(1),
            _ => null
        };
        tenant.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new CheckPaymentResponse
        {
            IsCompleted = true,
            Transaction = result
        };
    }
}
