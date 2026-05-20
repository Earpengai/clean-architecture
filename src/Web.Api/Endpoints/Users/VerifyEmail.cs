using Application.Abstractions.Messaging;
using Application.Users.VerifyEmail;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class VerifyEmail : IEndpoint
{
    public sealed record Request(string Token);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/verify-email", async (
            Request request,
            ICommandHandler<VerifyEmailCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new VerifyEmailCommand(request.Token);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
