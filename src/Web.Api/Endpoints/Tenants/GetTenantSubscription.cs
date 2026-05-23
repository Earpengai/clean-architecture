using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.GetTenantFeatures;
using Domain.Permissions;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetTenantSubscription : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenant/subscription", async (
            IQueryHandler<GetTenantFeaturesQuery, TenantFeaturesResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantFeaturesQuery();

            Result<TenantFeaturesResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(Permission.TenantsWrite)
        .WithTags(Tags.Subscription);
    }
}
