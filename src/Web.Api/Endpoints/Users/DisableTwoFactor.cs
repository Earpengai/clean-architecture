using Application.Abstractions.Messaging;
using Application.Users.DisableTwoFactor;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class DisableTwoFactor : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/disable-2fa", async (
            ICommandHandler<DisableTwoFactorCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DisableTwoFactorCommand();

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
