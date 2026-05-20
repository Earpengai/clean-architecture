using Application.Abstractions.Messaging;
using Application.Admin.DisableTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class DisableTenant : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/tenants/{tenantId:guid}/disable", async (
            Guid tenantId,
            ICommandHandler<DisableTenantCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DisableTenantCommand(tenantId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags("Admin");
    }
}
