using Application.Abstractions.Billing;
using Application.Abstractions.Data;
using Application.Abstractions.Jobs;
using Application.Billing.ProcessPayment;
using Domain.Payments;
using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Jobs;

internal sealed class ProcessPaymentJobHandler(
    IApplicationDbContext context,
    IBakongService bakongService,
    IBackgroundJobQueue jobQueue,
    ILogger<ProcessPaymentJobHandler> logger)
    : IBackgroundJobHandler<ProcessPaymentJob>
{
    private const int MaxAttempts = 8;

    public async Task Handle(ProcessPaymentJob job, CancellationToken cancellationToken)
    {
        Payment? payment = await context.Payments
            .FirstOrDefaultAsync(p => p.Id == job.PaymentId, cancellationToken);

        if (payment is null)
        {
            logger.LogWarning("Payment {PaymentId} not found, skipping verification", job.PaymentId);
            return;
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            logger.LogDebug("Payment {PaymentId} status is {Status}, skipping", job.PaymentId, payment.Status);
            return;
        }

        TransactionCheckResult? result = await bakongService.CheckTransactionAsync(payment.Md5Hash, cancellationToken);

        if (result is not null)
        {
            payment.Status = PaymentStatus.Completed;
            payment.TransactionHash = result.Hash;
            payment.CompletedAt = DateTime.UtcNow;

            Subscription? subscription = await context.Subscriptions
                .FirstOrDefaultAsync(s => s.TenantId == payment.TenantId, cancellationToken);

            if (subscription is null)
            {
                subscription = new Subscription
                {
                    Id = Guid.NewGuid(),
                    TenantId = payment.TenantId,
                    CreatedAt = DateTime.UtcNow
                };
                context.Subscriptions.Add(subscription);
            }

            subscription.SubscriptionPlanId = payment.SubscriptionPlanId;
            subscription.BillingPeriod = payment.BillingPeriod;
            subscription.Status = SubscriptionStatus.Active;
            subscription.ExpiresAt = payment.BillingPeriod switch
            {
                SubscriptionBillingPeriod.Monthly => DateTime.UtcNow.AddMonths(1),
                SubscriptionBillingPeriod.Yearly => DateTime.UtcNow.AddYears(1),
                _ => null
            };
            subscription.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Payment {PaymentId} completed for tenant {TenantId}, subscription activated",
                payment.Id,
                payment.TenantId);

            return;
        }

        if (job.Attempt >= MaxAttempts)
        {
            logger.LogWarning(
                "Payment {PaymentId} verification exhausted after {Attempts} attempts, leaving as Pending",
                job.PaymentId,
                MaxAttempts);

            return;
        }

        var nextJob = new ProcessPaymentJob(job.PaymentId, job.Attempt + 1);

        await jobQueue.EnqueueAsync(
            nextJob,
            scheduledAt: DateTime.UtcNow.AddSeconds(30),
            maxRetries: 1,
            cancellationToken);

        logger.LogDebug(
            "Payment {PaymentId} not yet confirmed, scheduled retry #{Attempt}",
            job.PaymentId,
            job.Attempt + 1);
    }
}
