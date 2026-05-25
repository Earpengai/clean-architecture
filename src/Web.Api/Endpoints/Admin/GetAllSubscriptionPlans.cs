using Application.Abstractions.Messaging;
using Application.Admin.GetAllSubscriptionPlans;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetAllSubscriptionPlans : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/subscription/plans", async (
            IQueryHandler<GetAllSubscriptionPlansQuery, List<SubscriptionPlanListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllSubscriptionPlansQuery();

            Result<List<SubscriptionPlanListItem>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
