using Application.Abstractions.Messaging;
using Application.Tenants.RemoveUserFromTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class RemoveUserFromTenant : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("tenants/users/{userId:guid}", async (
            Guid userId,
            ICommandHandler<RemoveUserFromTenantCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveUserFromTenantCommand(userId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.UsersDelete)
        .WithTags(Tags.Tenants);
    }
}
