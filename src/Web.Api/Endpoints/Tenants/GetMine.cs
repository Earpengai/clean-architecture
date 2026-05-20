using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Tenants.GetTenantsForUser;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetMine : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/mine", async (
            IQueryHandler<GetTenantsForUserQuery, List<TenantResponse>> handler,
            IUserContext userContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetTenantsForUserQuery(userContext.UserId);

            Result<List<TenantResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Tenants)
        .RequireAuthorization();
    }
}
