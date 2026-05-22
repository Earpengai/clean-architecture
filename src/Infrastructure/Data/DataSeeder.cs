using System.Collections.Immutable;
using Application.Abstractions.Data;
using Domain.SubscriptionFeatures;
using Domain.Tenants;
using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class DataSeeder(
    UserManager<User> userManager,
    IApplicationDbContext dbContext,
    IConfiguration configuration,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
    {
        await SeedSystemAdministratorAsync();
        await SeedPlanFeaturesAsync();
        await SeedPlanLimitsAsync();
    }

    private async Task SeedSystemAdministratorAsync()
    {
        string? adminEmail = configuration["AdminSettings:Email"];
        string? adminPassword = configuration["AdminSettings:Password"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning("Admin settings not configured. Skipping administrator seed");
            return;
        }

        User? admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin is not null && admin.IsSystemAdministrator)
        {
            logger.LogInformation("Administrator already exists");
            return;
        }

        var systemAdmin = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            UserName = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsSystemAdministrator = true,
            CreatedAt = DateTime.UtcNow
        };

        IdentityResult result = await userManager.CreateAsync(systemAdmin, adminPassword);

        if (result.Succeeded)
        {
            logger.LogInformation("Administrator created: {Email}", adminEmail);
        }
        else
        {
            foreach (IdentityError error in result.Errors)
            {
                logger.LogError("Identity error: {Code} - {Description}", error.Code, error.Description);
            }
        }
    }

    private async Task SeedPlanFeaturesAsync()
    {
        bool hasAny = await dbContext.PlanFeatures.AnyAsync();

        if (hasAny)
        {
            return;
        }

        foreach (SubscriptionPlan plan in Enum.GetValues<SubscriptionPlan>())
        {
            HashSet<string> features = DefaultPlanFeatures.GetDefaults(plan);

            foreach (string feature in SubscriptionFeature.All)
            {
                var planFeature = new PlanFeature
                {
                    Plan = plan,
                    Feature = feature,
                    IsEnabled = features.Contains(feature)
                };

                dbContext.PlanFeatures.Add(planFeature);
            }
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Default plan features seeded for {PlanCount} plans", Enum.GetValues<SubscriptionPlan>().Length);
    }

    private async Task SeedPlanLimitsAsync()
    {
        bool hasAny = await dbContext.PlanLimits.AnyAsync();

        if (hasAny)
        {
            return;
        }

        foreach (SubscriptionPlan plan in Enum.GetValues<SubscriptionPlan>())
        {
            IReadOnlyDictionary<string, int> limits = DefaultPlanLimits.GetDefaults(plan);

            foreach (string limit in SubscriptionLimit.All)
            {
                int value = limits.TryGetValue(limit, out int existingValue)
                    ? existingValue
                    : SubscriptionLimit.Unlimited;

                var planLimit = new PlanLimit
                {
                    Plan = plan,
                    Limit = limit,
                    Value = value
                };

                dbContext.PlanLimits.Add(planLimit);
            }
        }

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Default plan limits seeded for {PlanCount} plans", Enum.GetValues<SubscriptionPlan>().Length);
    }
}
