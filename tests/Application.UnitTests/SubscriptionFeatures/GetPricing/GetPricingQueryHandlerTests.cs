using Application.SubscriptionFeatures.GetPricing;
using SharedKernel;

namespace Application.UnitTests.SubscriptionFeatures.GetPricing;

public sealed class GetPricingQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnPricingList()
    {
        GetPricingQueryHandler handler = new();
        GetPricingQuery query = new();

        Result<List<PricingResponse>> result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Count.ShouldBe(5);
        result.Value.ShouldContain(p => p.Plan == "Free");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectFreePlanAmount()
    {
        GetPricingQueryHandler handler = new();

        Result<List<PricingResponse>> result = await handler.Handle(new GetPricingQuery(), CancellationToken.None);

        PricingResponse? freePlan = result.Value.FirstOrDefault(p => p.Plan == "Free");
        freePlan.ShouldNotBeNull();
        freePlan.Amount.ShouldBe(0m);
    }
}
