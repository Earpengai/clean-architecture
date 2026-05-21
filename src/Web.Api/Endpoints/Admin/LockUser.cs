using Application.Abstractions.Messaging;
using Application.Admin.LockUser;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class LockUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/users/{userId:guid}/lock", async (
            Guid userId,
            ICommandHandler<LockUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LockUserCommand(userId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Admin)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
