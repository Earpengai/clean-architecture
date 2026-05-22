using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.GetPlanLimits;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetPlanLimits : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/subscription/limits", async (
            IQueryHandler<GetPlanLimitsQuery, List<PlanLimitResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetPlanLimitsQuery();

            Result<List<PlanLimitResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
