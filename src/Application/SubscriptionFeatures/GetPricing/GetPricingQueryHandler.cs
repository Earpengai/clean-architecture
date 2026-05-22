using Application.Abstractions.Messaging;
using Domain.SubscriptionFeatures;
using Domain.Tenants;
using SharedKernel;

namespace Application.SubscriptionFeatures.GetPricing;

internal sealed class GetPricingQueryHandler
    : IQueryHandler<GetPricingQuery, List<PricingResponse>>
{
    public Task<Result<List<PricingResponse>>> Handle(
        GetPricingQuery query,
        CancellationToken cancellationToken)
    {
        var pricing = SubscriptionPricing.Prices
            .Select(kvp => new PricingResponse
            {
                Plan = kvp.Key.Plan.ToString(),
                BillingPeriod = kvp.Key.Period.ToString(),
                Amount = kvp.Value
            })
            .ToList();

        return Task.FromResult<Result<List<PricingResponse>>>(pricing);
    }
}
