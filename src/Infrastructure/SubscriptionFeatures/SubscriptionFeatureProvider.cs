using Application.Abstractions.Data;
using Application.Abstractions.SubscriptionFeatures;
using Domain.SubscriptionFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.SubscriptionFeatures;

internal sealed class SubscriptionFeatureProvider(IServiceScopeFactory serviceScopeFactory) : ISubscriptionFeatureProvider
{
    public async Task<HashSet<string>> GetEnabledFeaturesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();

        Guid? planId = await context.Subscriptions
            .Where(s => s.TenantId == tenantId)
            .Select(s => (Guid?)s.SubscriptionPlanId)
            .FirstOrDefaultAsync(cancellationToken);

        if (planId is null)
        {
            return [];
        }

        List<string> features = await context.PlanFeatures
            .Where(pf => pf.SubscriptionPlanId == planId.Value && pf.IsEnabled)
            .Select(pf => pf.Feature)
            .ToListAsync(cancellationToken);

        return [.. features];
    }

    public async Task<int?> GetLimitAsync(Guid tenantId, string limitKey, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();

        Guid? planId = await context.Subscriptions
            .Where(s => s.TenantId == tenantId)
            .FirstOrDefaultAsync(cancellationToken) is { } subscription
                ? subscription.SubscriptionPlanId
                : null;

        if (planId is null)
        {
            return null;
        }

        if (limitKey == SubscriptionLimit.MaxUsers)
        {
            int? override_ = await context.Subscriptions
                .Where(s => s.TenantId == tenantId)
                .Select(s => s.MaxUsersOverride)
                .FirstOrDefaultAsync(cancellationToken);

            if (override_ is not null)
            {
                return override_.Value;
            }
        }

        int? value = await context.PlanLimits
            .Where(pl => pl.SubscriptionPlanId == planId.Value && pl.Limit == limitKey)
            .Select(pl => (int?)pl.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return value;
    }
}
