using Application.Abstractions.SubscriptionFeatures;
using Finbuckle.MultiTenant;
using Infrastructure.Authentication;
using Infrastructure.Multitenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Authorization;

internal sealed class SubscriptionFeatureAuthorizationHandler(
    IServiceScopeFactory serviceScopeFactory,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<SubscriptionFeatureRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SubscriptionFeatureRequirement requirement)
    {
        if (context.User is not { Identity.IsAuthenticated: true })
        {
            return;
        }

        string? isAdminClaim = context.User.FindFirst("is_system_admin")?.Value;

        if (string.Equals(isAdminClaim, "TRUE", StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
            return;
        }

        AppTenantInfo? tenantInfo = httpContextAccessor
            .HttpContext?
            .GetMultiTenantContext<AppTenantInfo>()?
            .TenantInfo;

        if (tenantInfo is null || !Guid.TryParse(tenantInfo.Id, out Guid tenantId))
        {
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        ISubscriptionFeatureProvider featureProvider = scope.ServiceProvider
            .GetRequiredService<ISubscriptionFeatureProvider>();

        HashSet<string> features = await featureProvider.GetEnabledFeaturesAsync(tenantId);

        if (features.Contains(requirement.Feature))
        {
            context.Succeed(requirement);
        }
    }
}
