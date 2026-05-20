using Application.Abstractions.Messaging;
using Application.Tenants.GetRoles;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetRoles : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/roles", async (
            IQueryHandler<GetRolesQuery, List<RoleResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetRolesQuery();

            Result<List<RoleResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.RolesRead)
        .WithTags(Tags.Tenants);
    }
}
