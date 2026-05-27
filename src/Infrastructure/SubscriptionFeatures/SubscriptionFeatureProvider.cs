using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.SubscriptionFeatures;
using Domain.SubscriptionFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.SubscriptionFeatures;

internal sealed class SubscriptionFeatureProvider(
    IServiceScopeFactory serviceScopeFactory,
    ICacheService cacheService) : ISubscriptionFeatureProvider
{
    public async Task<HashSet<string>> GetEnabledFeaturesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"features:{tenantId}";

        HashSet<string>? cached = await cacheService.GetAsync<HashSet<string>>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();

        Guid? planId = await context.Subscriptions
            .Where(s => s.TenantId == tenantId)
            .Select(s => (Guid?)s.SubscriptionPlanId)
            .FirstOrDefaultAsync(cancellationToken);

        if (planId is null)
        {
            await cacheService.SetAsync(cacheKey, new HashSet<string>(), cancellationToken: cancellationToken);

            return [];
        }

        List<string> features = await context.PlanFeatures
            .Where(pf => pf.SubscriptionPlanId == planId.Value && pf.IsEnabled)
            .Select(pf => pf.Feature)
            .ToListAsync(cancellationToken);

        HashSet<string> result = [.. features];

        await cacheService.SetAsync(cacheKey, result, cancellationToken: cancellationToken);

        return result;
    }

    public async Task<int?> GetLimitAsync(Guid tenantId, string limitKey, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"limits:{tenantId}:{limitKey}";

        int? cached = await cacheService.GetAsync<int?>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

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
                await cacheService.SetAsync(cacheKey, override_.Value, cancellationToken: cancellationToken);

                return override_.Value;
            }
        }

        int? value = await context.PlanLimits
            .Where(pl => pl.SubscriptionPlanId == planId.Value && pl.Limit == limitKey)
            .Select(pl => (int?)pl.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (value is not null)
        {
            await cacheService.SetAsync(cacheKey, value, cancellationToken: cancellationToken);
        }

        return value;
    }
}
