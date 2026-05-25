using Application.Abstractions.Messaging;
using Application.Admin.GetAllSubscriptions;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetAllSubscriptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/subscriptions", async (
            IQueryHandler<GetAllSubscriptionsQuery, List<SubscriptionResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllSubscriptionsQuery();

            Result<List<SubscriptionResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
