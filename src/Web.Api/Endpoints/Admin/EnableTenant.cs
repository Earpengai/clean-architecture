using Application.Abstractions.Messaging;
using Application.Admin.EnableTenant;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class EnableTenant : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/tenants/{tenantId:guid}/enable", async (
            Guid tenantId,
            ICommandHandler<EnableTenantCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new EnableTenantCommand(tenantId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
