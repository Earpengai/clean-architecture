using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.GetTenantFeatures;

internal sealed class GetTenantFeaturesQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetTenantFeaturesQuery, TenantFeaturesResponse>
{
    public async Task<Result<TenantFeaturesResponse>> Handle(
        GetTenantFeaturesQuery query,
        CancellationToken cancellationToken)
    {
        if (userContext.TenantId is null)
        {
            return Result.Failure<TenantFeaturesResponse>(
                Domain.Users.UserErrors.Unauthorized());
        }

        Guid tenantId = userContext.TenantId.Value;

        Subscription? subscription = await context.Subscriptions
            .AsNoTracking()
            .Include(s => s.SubscriptionPlan)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<TenantFeaturesResponse>(TenantErrors.NotFound(tenantId));
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

        TenantFeaturesResponse response = new()
        {
            SubscriptionPlan = subscription.SubscriptionPlan?.Name ?? string.Empty,
            SubscriptionStatus = subscription.Status.ToString(),
            BillingPeriod = subscription.BillingPeriod.ToString(),
            SubscriptionExpiresAt = subscription.ExpiresAt?.ToString("O"),
            Features = features,
            Limits = limits
        };

        return response;
    }
}
