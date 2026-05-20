using System.Security.Claims;
using System.Text.Json;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Multitenancy;

internal sealed class TenantMembershipStrategy : IMultiTenantStrategy
{
    public async Task<string?> GetIdentifierAsync(object context)
    {
        if (context is not HttpContext httpContext)
        {
            return null;
        }

        string? tenantIdsClaim = httpContext.User.FindFirstValue("tenant_ids");

        if (string.IsNullOrEmpty(tenantIdsClaim))
        {
            return null;
        }

        string? activeTenant = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(activeTenant))
        {
            return null;
        }

        List<string>? tenantIdentifiers = JsonSerializer.Deserialize<List<string>>(tenantIdsClaim);

        if (tenantIdentifiers is null || tenantIdentifiers.Count == 0)
        {
            return null;
        }

        if (tenantIdentifiers.Contains(activeTenant))
        {
            return activeTenant;
        }

        return null;
    }
}
