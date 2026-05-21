using Application.Abstractions.Messaging;
using Application.Users.EnableTwoFactor;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class EnableTwoFactor : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/enable-2fa", async (
            ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new EnableTwoFactorCommand();

            Result<EnableTwoFactorResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
