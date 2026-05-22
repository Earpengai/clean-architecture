using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authorization;

internal sealed class SubscriptionFeatureRequirement : IAuthorizationRequirement
{
    public SubscriptionFeatureRequirement(string feature)
    {
        Feature = feature;
    }

    public string Feature { get; }
}
