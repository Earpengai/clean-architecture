using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.SubscriptionFeatures;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.GetAvailablePlans;

internal sealed class GetAvailablePlansQueryHandler(
    IApplicationDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetAvailablePlansQuery, List<AvailablePlanResponse>>
{
    public async Task<Result<List<AvailablePlanResponse>>> Handle(
        GetAvailablePlansQuery query,
        CancellationToken cancellationToken)
    {
        List<SubscriptionPlan> plans = await context.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        List<Guid> userOwnedTenantIds = await context.Memberships
            .Where(m => m.UserId == userContext.UserId && m.Role.Name == "Owner")
            .Select(m => m.TenantId)
            .ToListAsync(cancellationToken);

        var ownedCounts = await context.Subscriptions
            .Where(s => userOwnedTenantIds.Contains(s.TenantId))
            .GroupBy(s => s.SubscriptionPlanId)
            .Select(g => new { PlanId = g.Key, OwnedCount = g.Count() })
            .ToListAsync(cancellationToken);

        var ownedByPlan = ownedCounts
            .ToDictionary(x => x.PlanId, x => x.OwnedCount);

        List<AvailablePlanResponse> result = [];
        foreach (SubscriptionPlan plan in plans)
        {
            List<string> features = await context.PlanFeatures
                .Where(pf => pf.SubscriptionPlanId == plan.Id && pf.IsEnabled)
                .Select(pf => pf.Feature)
                .ToListAsync(cancellationToken);

            Dictionary<string, int> limits = await context.PlanLimits
                .Where(pl => pl.SubscriptionPlanId == plan.Id)
                .ToDictionaryAsync(pl => pl.Limit, pl => pl.Value, cancellationToken);

            int maxTenants = limits.TryGetValue(SubscriptionLimit.MaxTenantsPerUser, out int value)
                ? value
                : SubscriptionLimit.Unlimited;

            int owned = ownedByPlan.TryGetValue(plan.Id, out int count) ? count : 0;

            int remainingQuota = maxTenants == SubscriptionLimit.Unlimited
                ? SubscriptionLimit.Unlimited
                : Math.Max(0, maxTenants - owned);

            result.Add(new AvailablePlanResponse(
                plan.Id,
                plan.Name,
                plan.Description,
                plan.PriceMonthly,
                plan.PriceYearly,
                plan.TrialDays,
                remainingQuota,
                [.. features],
                limits));
        }

        return result;
    }
}