using Application.Abstractions.Messaging;
using Application.Tenants.UpdateRole;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class UpdateRole : IEndpoint
{
    public sealed record Request(string Name, string? Description, List<string> Permissions);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("tenants/roles/{roleId:guid}", async (
            Guid roleId,
            Request request,
            ICommandHandler<UpdateRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateRoleCommand(roleId, request.Name, request.Description, request.Permissions);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.RolesWrite)
        .WithTags(Tags.Tenants);
    }
}
