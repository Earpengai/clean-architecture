using Application.Abstractions.Messaging;
using Application.Admin.UnlockUser;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class UnlockUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/users/{userId:guid}/unlock", async (
            Guid userId,
            ICommandHandler<UnlockUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UnlockUserCommand(userId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Admin)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
