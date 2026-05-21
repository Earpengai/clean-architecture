using Application.Abstractions.Messaging;
using Application.Users.Login;
using Application.Users.LoginTwoFactor;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class LoginTwoFactor : IEndpoint
{
    public sealed record Request(Guid UserId, string TwoFactorToken, string Code);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login-2fa", async (
            Request request,
            ICommandHandler<LoginTwoFactorCommand, LoginResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginTwoFactorCommand(request.UserId, request.TwoFactorToken, request.Code);

            Result<LoginResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .WithTags(Tags.Users);
    }
}
