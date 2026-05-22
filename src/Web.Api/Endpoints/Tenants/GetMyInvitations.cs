using Application.Abstractions.Messaging;
using Application.Tenants.GetMyInvitations;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetMyInvitations : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("invitations/my", async (
            IQueryHandler<GetMyInvitationsQuery, List<MyInvitationResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetMyInvitationsQuery();

            Result<List<MyInvitationResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Tenants);
    }
}
