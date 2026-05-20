using Application.Abstractions.Messaging;
using Application.Tenants.InviteUser;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class InviteUser : IEndpoint
{
    public sealed record Request(string Email, Guid RoleId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/invitations", async (
            Request request,
            ICommandHandler<InviteUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new InviteUserCommand(request.Email, request.RoleId);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.UsersWrite)
        .WithTags(Tags.Tenants);
    }
}
