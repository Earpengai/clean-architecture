using Application.Abstractions.Messaging;
using Application.Users.ResetPassword;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ResetPassword : IEndpoint
{
    public sealed record Request(Guid UserId, string Token, string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/reset-password", async (
            Request request,
            ICommandHandler<ResetPasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ResetPasswordCommand(request.UserId, request.Token, request.NewPassword);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .ExcludeFromMultiTenantResolution()
        .WithTags(Tags.Users);
    }
}
