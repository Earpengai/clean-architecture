using Application.Abstractions.Messaging;
using Application.Admin.DeactivateSubscriptionPlan;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class DeactivateSubscriptionPlan : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/subscription/plans/{planId:guid}/deactivate", async (
            Guid planId,
            ICommandHandler<DeactivateSubscriptionPlanCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeactivateSubscriptionPlanCommand(planId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
