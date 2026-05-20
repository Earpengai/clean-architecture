using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

internal sealed class DataSeeder(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
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

        bool adminExists = await context.Users
            .AnyAsync(u => u.Email == adminEmail && u.IsSystemAdministrator);

        if (adminExists)
        {
            logger.LogInformation("Administrator already exists");
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            PasswordHash = passwordHasher.Hash(adminPassword),
            EmailVerified = true,
            IsSystemAdministrator = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();

        logger.LogInformation("Administrator created: {Email}", adminEmail);
    }
}
