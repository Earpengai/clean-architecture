using Application.Abstractions.Messaging;
using Application.Admin.GetAllTenants;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class GetAllTenants : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/tenants", async (
            IQueryHandler<GetAllTenantsAdminQuery, List<TenantAdminResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetAllTenantsAdminQuery();

            Result<List<TenantAdminResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags("Admin");
    }
}
