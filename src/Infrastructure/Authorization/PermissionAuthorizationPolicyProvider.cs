using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Infrastructure.Authorization;

internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _authorizationOptions;

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
        _authorizationOptions = options.Value;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        IAuthorizationRequirement requirement = policyName.StartsWith("feature:", StringComparison.Ordinal)
            ? new SubscriptionFeatureRequirement(policyName)
            : new PermissionRequirement(policyName);

        AuthorizationPolicy newPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(requirement)
            .Build();

        _authorizationOptions.AddPolicy(policyName, newPolicy);

        return newPolicy;
    }
}
