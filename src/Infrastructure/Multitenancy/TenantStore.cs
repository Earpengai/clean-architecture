using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Multitenancy;

internal sealed class TenantStore : IMultiTenantStore<AppTenantInfo>
{
    private readonly IServiceProvider _serviceProvider;

    public TenantStore(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<AppTenantInfo?> TryGetByIdentifierAsync(string identifier)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        Domain.Tenants.Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .Include(t => t.Subscription)
            .ThenInclude(s => s!.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Identifier == identifier);

        if (tenant is null)
        {
            return null;
        }

        return MapToAppTenantInfo(tenant);
    }

    public async Task<AppTenantInfo?> TryGetAsync(string id)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        if (!Guid.TryParse(id, out Guid tenantId))
        {
            return null;
        }

        Domain.Tenants.Tenant? tenant = await context.Tenants
            .AsNoTracking()
            .Include(t => t.Subscription)
            .ThenInclude(s => s!.SubscriptionPlan)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant is null)
        {
            return null;
        }

        return MapToAppTenantInfo(tenant);
    }

    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        List<Domain.Tenants.Tenant> tenants = await context.Tenants
            .AsNoTracking()
            .Include(t => t.Subscription)
            .ThenInclude(s => s!.SubscriptionPlan)
            .ToListAsync();

        return tenants.Select(MapToAppTenantInfo);
    }

    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync(int take, int skip)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        List<Domain.Tenants.Tenant> tenants = await context.Tenants
            .AsNoTracking()
            .Include(t => t.Subscription)
            .ThenInclude(s => s!.SubscriptionPlan)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return tenants.Select(MapToAppTenantInfo);
    }

    public async Task<bool> TryAddAsync(AppTenantInfo tenantInfo)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        bool exists = await context.Tenants.AnyAsync(t => t.Identifier == tenantInfo.Identifier);

        if (exists)
        {
            return false;
        }

        var tenant = new Domain.Tenants.Tenant
        {
            Id = Guid.Parse(tenantInfo.Id!),
            Name = tenantInfo.Name!,
            Identifier = tenantInfo.Identifier!,
            CreatedAt = DateTime.UtcNow
        };

        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TryRemoveAsync(string identifier)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        Domain.Tenants.Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Identifier == identifier);

        if (tenant is null)
        {
            return false;
        }

        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> TryUpdateAsync(AppTenantInfo tenantInfo)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        Database.ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<Database.ApplicationDbContext>();

        Domain.Tenants.Tenant? tenant = await context.Tenants
            .FirstOrDefaultAsync(t => t.Identifier == tenantInfo.Identifier);

        if (tenant is null)
        {
            return false;
        }

        tenant.Name = tenantInfo.Name!;
        tenant.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return true;
    }

    private static AppTenantInfo MapToAppTenantInfo(Domain.Tenants.Tenant tenant)
    {
        return new AppTenantInfo
        {
            Id = tenant.Id.ToString(),
            Name = tenant.Name,
            Identifier = tenant.Identifier,
            SubscriptionPlan = tenant.Subscription?.SubscriptionPlan?.Name ?? string.Empty,
            SubscriptionStatus = tenant.Subscription?.Status.ToString() ?? string.Empty,
            MaxUsersOverride = tenant.Subscription?.MaxUsersOverride
        };
    }
}
