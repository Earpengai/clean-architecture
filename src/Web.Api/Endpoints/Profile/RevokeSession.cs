using Application.Abstractions.Messaging;
using Application.Users.RevokeSession;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profile;

internal sealed class RevokeSession : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("profile/sessions/{sessionId:guid}", async (
            Guid sessionId,
            ICommandHandler<RevokeSessionCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RevokeSessionCommand(sessionId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Profile)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
