using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Infrastructure.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Database;

internal sealed class TenantSaveInterceptor(
    IMultiTenantContextAccessor<AppTenantInfo> multiTenantContextAccessor)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetTenantIdOnNewEntities(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTenantIdOnNewEntities(eventData.Context);
        return new ValueTask<InterceptionResult<int>>(result);
    }

    private void SetTenantIdOnNewEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        string? tenantId = multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Id;

        if (tenantId is null)
        {
            return;
        }

        var parsedTenantId = Guid.Parse(tenantId);

        foreach (EntityEntry entry in context.ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Added))
        {
            if (entry.Metadata.FindProperty("TenantId") is not null)
            {
                entry.Property("TenantId").CurrentValue = parsedTenantId;
            }
        }
    }
}
