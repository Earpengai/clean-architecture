using Application.Abstractions.Messaging;
using Application.Users.ChangePassword;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profile;

internal sealed class ChangePassword : IEndpoint
{
    public sealed record Request(string CurrentPassword, string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("profile/change-password", async (
            Request request,
            ICommandHandler<ChangePasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Profile)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
