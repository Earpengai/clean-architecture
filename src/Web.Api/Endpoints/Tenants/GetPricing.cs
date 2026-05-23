using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.GetPricing;
using Domain.Permissions;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetPricing : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenant/subscription/pricing", async (
            IQueryHandler<GetPricingQuery, List<PricingResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            GetPricingQuery query = new();

            Result<List<PricingResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(Permission.TenantsWrite)
        .WithTags(Tags.Subscription);
    }
}
