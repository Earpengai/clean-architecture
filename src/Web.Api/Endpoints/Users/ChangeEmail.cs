using Application.Abstractions.Messaging;
using Application.Users.ChangeEmail;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ChangeEmail : IEndpoint
{
    public sealed record Request(string NewEmail);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/change-email", async (
            Request request,
            ICommandHandler<ChangeEmailCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangeEmailCommand(request.NewEmail);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
