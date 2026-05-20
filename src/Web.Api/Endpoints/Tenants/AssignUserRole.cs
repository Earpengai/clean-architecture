using Application.Abstractions.Messaging;
using Application.Tenants.AssignUserRole;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class AssignUserRole : IEndpoint
{
    public sealed record Request(Guid RoleId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/users/{userId:guid}/roles", async (
            Guid userId,
            Request request,
            ICommandHandler<AssignUserRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AssignUserRoleCommand(userId, request.RoleId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.RolesWrite)
        .WithTags(Tags.Tenants);
    }
}
