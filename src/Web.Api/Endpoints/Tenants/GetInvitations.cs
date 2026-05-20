using Application.Abstractions.Messaging;
using Application.Tenants.GetInvitations;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetInvitations : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenants/invitations", async (
            IQueryHandler<GetInvitationsQuery, List<InvitationResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetInvitationsQuery();

            Result<List<InvitationResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.UsersRead)
        .WithTags(Tags.Tenants);
    }
}
