using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.GetPlanFeatures;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetPlanFeatures : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/subscription/features", async (
            IQueryHandler<GetPlanFeaturesQuery, List<PlanFeatureResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPlanFeaturesQuery();

            Result<List<PlanFeatureResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
