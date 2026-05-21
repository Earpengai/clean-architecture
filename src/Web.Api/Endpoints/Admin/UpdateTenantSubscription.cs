using Application.Abstractions.Messaging;
using Application.Admin.UpdateTenantSubscription;
using Domain.Tenants;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class UpdateTenantSubscription : IEndpoint
{
    public sealed record Request(
        SubscriptionPlan SubscriptionPlan,
        SubscriptionStatus SubscriptionStatus,
        int SeatCount);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/tenants/{tenantId:guid}/subscription", async (
            Guid tenantId,
            Request request,
            ICommandHandler<UpdateTenantSubscriptionCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTenantSubscriptionCommand(
                tenantId,
                request.SubscriptionPlan,
                request.SubscriptionStatus,
                request.SeatCount);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
