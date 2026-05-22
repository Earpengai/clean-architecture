namespace Application.Abstractions.SubscriptionFeatures;

public interface ISubscriptionFeatureProvider
{
    Task<HashSet<string>> GetEnabledFeaturesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<int?> GetLimitAsync(Guid tenantId, string limitKey, CancellationToken cancellationToken = default);
}
