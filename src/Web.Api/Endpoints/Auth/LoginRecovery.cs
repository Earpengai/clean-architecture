using Application.Abstractions.Messaging;
using Application.Users.Login;
using Application.Users.LoginRecovery;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class LoginRecovery : IEndpoint
{
    public sealed record Request(Guid UserId, string RecoveryCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login-recovery", async (
            Request request,
            ICommandHandler<LoginRecoveryCommand, LoginResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginRecoveryCommand(request.UserId, request.RecoveryCode);

            Result<LoginResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
