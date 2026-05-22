using Application.Abstractions.Data;
using Application.Abstractions.SubscriptionFeatures;
using Domain.Tenants;
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

        SubscriptionPlan? plan = await context.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => (SubscriptionPlan?)t.SubscriptionPlan)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan is null)
        {
            return [];
        }

        List<string> features = await context.PlanFeatures
            .Where(pf => pf.Plan == plan.Value && pf.IsEnabled)
            .Select(pf => pf.Feature)
            .ToListAsync(cancellationToken);

        return [.. features];
    }

    public async Task<int?> GetLimitAsync(Guid tenantId, string limitKey, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<IApplicationDbContext>();

        SubscriptionPlan? plan = await context.Tenants
            .Where(t => t.Id == tenantId)
            .Select(t => (SubscriptionPlan?)t.SubscriptionPlan)
            .FirstOrDefaultAsync(cancellationToken);

        if (plan is null)
        {
            return null;
        }

        int? value = await context.PlanLimits
            .Where(pl => pl.Plan == plan.Value && pl.Limit == limitKey)
            .Select(pl => (int?)pl.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return value;
    }
}
