using Application.Abstractions.Messaging;
using Application.Tenants.AcceptInvitation;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class AcceptInvitationAuthenticated : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("invitations/{token}/accept-authenticated", async (
            string token,
            ICommandHandler<AcceptInvitationForUserCommand, AcceptInvitationResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AcceptInvitationForUserCommand(token);

            Result<AcceptInvitationResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Tenants);
    }
}
