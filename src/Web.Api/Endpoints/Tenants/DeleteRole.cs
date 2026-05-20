using Application.Abstractions.Messaging;
using Application.Tenants.DeleteRole;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class DeleteRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("tenants/roles/{roleId:guid}", async (
            Guid roleId,
            ICommandHandler<DeleteRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteRoleCommand(roleId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.RolesDelete)
        .WithTags(Tags.Tenants);
    }
}
