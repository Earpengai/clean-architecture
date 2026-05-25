using Application.Abstractions.Messaging;
using Application.Admin.GetTenantSubscription;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetTenantSubscription : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/tenants/{tenantId:guid}/subscription", async (
            Guid tenantId,
            IQueryHandler<GetTenantSubscriptionQuery, TenantSubscriptionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantSubscriptionQuery(tenantId);

            Result<TenantSubscriptionResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
