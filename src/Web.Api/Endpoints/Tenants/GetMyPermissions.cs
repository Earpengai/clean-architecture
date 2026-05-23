using Application.Abstractions.Messaging;
using Application.Tenants.GetMyPermissions;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetMyPermissions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/my-permissions", async (
            IQueryHandler<GetMyPermissionsQuery, GetMyPermissionsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyPermissionsQuery();

            Result<GetMyPermissionsResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Tenants)
        .RequireAuthorization();
    }
}
