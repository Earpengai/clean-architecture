using Application.Abstractions.Data;
using Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Billing;

internal sealed class SubscriptionExpirationService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SubscriptionExpirationService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExpireSubscriptionsAsync(stoppingToken);

        using PeriodicTimer timer = new(TimeSpan.FromHours(1));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExpireSubscriptionsAsync(stoppingToken);
        }
    }

    private async Task ExpireSubscriptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            IApplicationDbContext context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            DateTime now = DateTime.UtcNow;

            List<Tenant> expiredTenants = await context.Tenants
                .Where(t => t.SubscriptionExpiresAt < now
                            && t.SubscriptionStatus == SubscriptionStatus.Active
                            && t.SubscriptionPlan != SubscriptionPlan.Free)
                .ToListAsync(cancellationToken);

            if (expiredTenants.Count == 0)
            {
                return;
            }

            foreach (Tenant tenant in expiredTenants)
            {
                tenant.SubscriptionStatus = SubscriptionStatus.Expired;
                tenant.UpdatedAt = now;
            }

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Expired {Count} tenant subscriptions", expiredTenants.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to expire subscriptions");
        }
    }
}
