using Application.Abstractions.Messaging;
using Application.Users.RequestEmailVerification;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RequestEmailVerification : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/request-verification", async (
            ICommandHandler<RequestEmailVerificationCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RequestEmailVerificationCommand();

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
