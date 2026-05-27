using Application.Abstractions.Caching;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(
    IServiceScopeFactory serviceScopeFactory,
    ICacheService cacheService)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        string cacheKey = $"permissions:{userId}";

        HashSet<string>? cached = await cacheService.GetAsync<HashSet<string>>(cacheKey);

        if (cached is not null)
        {
            return cached;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        List<string> permissions = await context.Memberships
            .Where(m => m.UserId == userId)
            .SelectMany(m => m.Role.Permissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();

        HashSet<string> result = [.. permissions];

        await cacheService.SetAsync(cacheKey, result);

        return result;
    }
}
