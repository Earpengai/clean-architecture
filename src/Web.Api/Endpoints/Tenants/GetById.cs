using Application.Abstractions.Messaging;
using Application.Tenants.GetTenantById;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/{tenantId:guid}", async (
            Guid tenantId,
            IQueryHandler<GetTenantByIdQuery, TenantResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantByIdQuery(tenantId);

            Result<TenantResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Tenants)
        .RequireAuthorization();
    }
}
