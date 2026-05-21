using Application.Abstractions.Messaging;
using Application.Tenants.AcceptInvitation;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class AcceptInvitation : IEndpoint
{
    public sealed record Request(string Token, string FirstName, string LastName, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("invitations/{token}/accept", async (
            string token,
            Request request,
            ICommandHandler<AcceptInvitationCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AcceptInvitationCommand(
                token,
                request.FirstName,
                request.LastName,
                request.Password);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Tenants);
    }
}
