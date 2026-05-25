using Application.Abstractions.Messaging;
using Application.Admin.GetSubscriptionPlanById;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetSubscriptionPlanById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/subscription/plans/{planId:guid}", async (
            Guid planId,
            IQueryHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDetail> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSubscriptionPlanByIdQuery(planId);

            Result<SubscriptionPlanDetail> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
