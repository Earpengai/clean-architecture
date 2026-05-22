using Application.Abstractions.Messaging;
using Application.Users.VerifyEmail;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class VerifyEmail : IEndpoint
{
    public sealed record Request(Guid UserId, string Token);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/verify-email", async (
            Request request,
            ICommandHandler<VerifyEmailCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new VerifyEmailCommand(request.UserId, request.Token);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
