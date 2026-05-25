using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.SubscriptionFeatures.GetPricing;

internal sealed class GetPricingQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetPricingQuery, List<PricingResponse>>
{
    public async Task<Result<List<PricingResponse>>> Handle(
        GetPricingQuery query,
        CancellationToken cancellationToken)
    {
        List<SubscriptionPlan> plans = await context.SubscriptionPlans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(cancellationToken);

        List<PricingResponse> pricing = [];

        foreach (SubscriptionPlan plan in plans)
        {
            pricing.Add(new PricingResponse
            {
                PlanId = plan.Id,
                Plan = plan.Name,
                BillingPeriod = "Monthly",
                Amount = plan.PriceMonthly
            });

            if (plan.PriceYearly > 0)
            {
                pricing.Add(new PricingResponse
                {
                    PlanId = plan.Id,
                    Plan = plan.Name,
                    BillingPeriod = "Yearly",
                    Amount = plan.PriceYearly
                });
            }
        }

        return pricing;
    }
}
