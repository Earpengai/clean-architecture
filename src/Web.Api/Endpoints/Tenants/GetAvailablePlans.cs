using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.GetAvailablePlans;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetAvailablePlans : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/available-plans", async (
            IQueryHandler<GetAvailablePlansQuery, List<AvailablePlanResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAvailablePlansQuery();

            Result<List<AvailablePlanResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Tenants);
    }
}