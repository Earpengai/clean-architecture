using Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class DataSeeder(
    UserManager<User> userManager,
    IConfiguration configuration,
    ILogger<DataSeeder> logger)
{
    public async Task SeedAsync()
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
}
