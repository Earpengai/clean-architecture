using Application.Abstractions.Messaging;
using Application.Tenants.CreateRole;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class CreateRole : IEndpoint
{
    public sealed record Request(string Name, string? Description, List<string> Permissions);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/roles", async (
            Request request,
            ICommandHandler<CreateRoleCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateRoleCommand(request.Name, request.Description, request.Permissions);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .HasPermission(Domain.Permissions.Permission.RolesWrite)
        .WithTags(Tags.Tenants);
    }
}
