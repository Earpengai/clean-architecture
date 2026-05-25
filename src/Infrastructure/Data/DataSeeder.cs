using Application.Abstractions.Data;
using Domain.SubscriptionFeatures;
using Domain.Subscriptions;
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
        await SeedSubscriptionPlansAsync();
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

    private async Task SeedSubscriptionPlansAsync()
    {
        bool hasAny = await dbContext.SubscriptionPlans.AnyAsync();

        if (hasAny)
        {
            return;
        }

        var free = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Free",
            Description = "Free plan with basic features",
            PriceMonthly = 0m,
            PriceYearly = 0m,
            TrialDays = 0,
            SortOrder = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var pro = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Pro",
            Description = "Pro plan with advanced features",
            PriceMonthly = 0.01m,
            PriceYearly = 299.99m,
            TrialDays = 14,
            SortOrder = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var enterprise = new SubscriptionPlan
        {
            Id = Guid.NewGuid(),
            Name = "Enterprise",
            Description = "Enterprise plan with all features",
            PriceMonthly = 99.99m,
            PriceYearly = 999.99m,
            TrialDays = 14,
            SortOrder = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.SubscriptionPlans.Add(free);
        dbContext.SubscriptionPlans.Add(pro);
        dbContext.SubscriptionPlans.Add(enterprise);

        await dbContext.SaveChangesAsync();

        SeedPlanFeatures(free);
        SeedPlanFeatures(pro);
        SeedPlanFeatures(enterprise);

        SeedPlanLimits(free);
        SeedPlanLimits(pro);
        SeedPlanLimits(enterprise);

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Default subscription plans, features, and limits seeded for 3 plans");
    }

    private void SeedPlanFeatures(SubscriptionPlan plan)
    {
        HashSet<string> features = DefaultPlanFeatures.GetDefaults(plan.Name);

        foreach (string feature in SubscriptionFeature.All)
        {
            var planFeature = new PlanFeature
            {
                SubscriptionPlanId = plan.Id,
                Feature = feature,
                IsEnabled = features.Contains(feature)
            };

            dbContext.PlanFeatures.Add(planFeature);
        }
    }

    private void SeedPlanLimits(SubscriptionPlan plan)
    {
        IReadOnlyDictionary<string, int> limits = DefaultPlanLimits.GetDefaults(plan.Name);

        foreach (string limit in SubscriptionLimit.All)
        {
            int value = limits.TryGetValue(limit, out int existingValue)
                ? existingValue
                : SubscriptionLimit.Unlimited;

            var planLimit = new PlanLimit
            {
                SubscriptionPlanId = plan.Id,
                Limit = limit,
                Value = value
            };

            dbContext.PlanLimits.Add(planLimit);
        }
    }
}
