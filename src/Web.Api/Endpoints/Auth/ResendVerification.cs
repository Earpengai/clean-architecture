using Application.Abstractions.Messaging;
using Application.Users.ResendVerification;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class ResendVerification : IEndpoint
{
    public sealed record Request(Guid UserId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/resend-verification", async (
            Request request,
            ICommandHandler<ResendVerificationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ResendVerificationCommand(request.UserId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
