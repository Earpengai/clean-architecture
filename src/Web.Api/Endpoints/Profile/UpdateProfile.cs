using Application.Abstractions.Messaging;
using Application.Users.UpdateProfile;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profile;

internal sealed class UpdateProfile : IEndpoint
{
    public sealed record Request(string FirstName, string LastName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profile", async (
            Request request,
            ICommandHandler<UpdateUserProfileCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateUserProfileCommand(request.FirstName, request.LastName);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Profile)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
