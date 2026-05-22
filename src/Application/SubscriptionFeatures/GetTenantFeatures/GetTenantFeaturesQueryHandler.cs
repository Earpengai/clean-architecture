using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
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

        Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantFeaturesResponse>(TenantErrors.NotFound(tenantId));
        }

        List<FeatureState> features = await context.PlanFeatures
            .AsNoTracking()
            .Where(pf => pf.Plan == tenant.SubscriptionPlan)
            .OrderBy(pf => pf.Feature)
            .Select(pf => new FeatureState
            {
                Feature = pf.Feature,
                IsEnabled = pf.IsEnabled
            })
            .ToListAsync(cancellationToken);

        List<LimitState> limits = await context.PlanLimits
            .AsNoTracking()
            .Where(pl => pl.Plan == tenant.SubscriptionPlan)
            .OrderBy(pl => pl.Limit)
            .Select(pl => new LimitState
            {
                Limit = pl.Limit,
                Value = pl.Value
            })
            .ToListAsync(cancellationToken);

        TenantFeaturesResponse response = new()
        {
            SubscriptionPlan = tenant.SubscriptionPlan.ToString(),
            SubscriptionStatus = tenant.SubscriptionStatus.ToString(),
            BillingPeriod = tenant.BillingPeriod.ToString(),
            SubscriptionExpiresAt = tenant.SubscriptionExpiresAt?.ToString("O"),
            Features = features,
            Limits = limits
        };

        return response;
    }
}
