using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class PermissionProvider(IServiceScopeFactory serviceScopeFactory)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        List<string> permissions = await context.Memberships
            .Where(m => m.UserId == userId)
            .SelectMany(m => m.Role.Permissions)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();

        return [.. permissions];
    }
}
