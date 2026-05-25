using Application.Abstractions.Data;
using Domain.Subscriptions;
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

            List<Subscription> expiredSubscriptions = await context.Subscriptions
                .Where(s => s.ExpiresAt < now && s.Status != SubscriptionStatus.Expired)
                .ToListAsync(cancellationToken);

            if (expiredSubscriptions.Count == 0)
            {
                return;
            }

            foreach (Subscription subscription in expiredSubscriptions)
            {
                subscription.Status = SubscriptionStatus.Expired;
                subscription.UpdatedAt = now;
            }

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Expired {Count} subscriptions", expiredSubscriptions.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to expire subscriptions");
        }
    }
}
