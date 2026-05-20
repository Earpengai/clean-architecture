using Application.Abstractions.Authentication;
using Finbuckle.MultiTenant;
using Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Authentication;

internal sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        _httpContextAccessor
            .HttpContext?
            .User
            .GetUserId() ??
        throw new UserContextUnavailableException();

    public Guid? TenantId
    {
        get
        {
            AppTenantInfo? tenantInfo = _httpContextAccessor
                .HttpContext?
                .GetMultiTenantContext<AppTenantInfo>()?
                .TenantInfo;

            if (tenantInfo is null || !Guid.TryParse(tenantInfo.Id, out Guid tenantId))
            {
                return null;
            }

            return tenantId;
        }
    }

    public bool IsSystemAdministrator
    {
        get
        {
            string? claim = _httpContextAccessor
                .HttpContext?
                .User
                .FindFirst("is_system_admin")?
                .Value;

            return string.Equals(claim, "TRUE", StringComparison.OrdinalIgnoreCase);
        }
    }
}
