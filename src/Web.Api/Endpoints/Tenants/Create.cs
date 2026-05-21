using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Tenants.CreateTenant;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class Create : IEndpoint
{
    public sealed record Request(string Name, string Identifier);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants", async (
            Request request,
            ICommandHandler<CreateTenantCommand, Guid> handler,
            IUserContext userContext,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTenantCommand(
                request.Name,
                request.Identifier,
                userContext.UserId);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Tenants)
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution();
    }
}
