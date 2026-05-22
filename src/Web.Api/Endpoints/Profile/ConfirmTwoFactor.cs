using Application.Abstractions.Messaging;
using Application.Users.ConfirmTwoFactor;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profile;

internal sealed class ConfirmTwoFactor : IEndpoint
{
    public sealed record Request(string Code);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("profile/confirm-2fa", async (
            Request request,
            ICommandHandler<ConfirmTwoFactorCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmTwoFactorCommand(request.Code);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Profile)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
