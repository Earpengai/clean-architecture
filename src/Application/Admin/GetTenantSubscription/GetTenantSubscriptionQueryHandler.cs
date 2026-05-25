using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Tenants;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Admin.GetTenantSubscription;

internal sealed class GetTenantSubscriptionQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTenantSubscriptionQuery, TenantSubscriptionResponse>
{
    public async Task<Result<TenantSubscriptionResponse>> Handle(
        GetTenantSubscriptionQuery query,
        CancellationToken cancellationToken)
    {
        if (!userContext.IsSystemAdministrator)
        {
            return Result.Failure<TenantSubscriptionResponse>(UserErrors.Unauthorized());
        }

        Subscription? subscription = await context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Tenant)
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.TenantId == query.TenantId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<TenantSubscriptionResponse>(
                TenantErrors.NotFound(query.TenantId));
        }

        List<FeatureState> features = await context.PlanFeatures
            .AsNoTracking()
            .Where(pf => pf.SubscriptionPlanId == subscription.SubscriptionPlanId)
            .OrderBy(pf => pf.Feature)
            .Select(pf => new FeatureState
            {
                Feature = pf.Feature,
                IsEnabled = pf.IsEnabled
            })
            .ToListAsync(cancellationToken);

        List<LimitState> limits = await context.PlanLimits
            .AsNoTracking()
            .Where(pl => pl.SubscriptionPlanId == subscription.SubscriptionPlanId)
            .OrderBy(pl => pl.Limit)
            .Select(pl => new LimitState
            {
                Limit = pl.Limit,
                Value = pl.Value
            })
            .ToListAsync(cancellationToken);

        return new TenantSubscriptionResponse
        {
            SubscriptionId = subscription.Id,
            TenantId = subscription.TenantId,
            TenantName = subscription.Tenant?.Name ?? string.Empty,
            PlanName = subscription.SubscriptionPlan?.Name ?? string.Empty,
            Status = subscription.Status.ToString(),
            BillingPeriod = subscription.BillingPeriod.ToString(),
            ExpiresAt = subscription.ExpiresAt,
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt,
            Features = features,
            Limits = limits
        };
    }
}
