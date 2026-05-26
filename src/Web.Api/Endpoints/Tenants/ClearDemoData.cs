using Application.Abstractions.Messaging;
using Application.Tenants.ClearDemoData;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class ClearDemoData : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenants/{tenantId:guid}/clear-demo-data", async (
            Guid tenantId,
            ICommandHandler<ClearDemoDataCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ClearDemoDataCommand(tenantId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Tenants)
        .HasPermission("tenants:write")
        .RequireAuthorization();
    }
}
