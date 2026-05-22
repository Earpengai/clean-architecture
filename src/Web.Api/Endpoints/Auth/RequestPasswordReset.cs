using Application.Abstractions.Messaging;
using Application.Users.RequestPasswordReset;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auth;

internal sealed class RequestPasswordReset : IEndpoint
{
    public sealed record Request(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/request-password-reset", async (
            Request request,
            ICommandHandler<RequestPasswordResetCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RequestPasswordResetCommand(request.Email);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireRateLimiting("AuthRateLimit")
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Auth);
    }
}
