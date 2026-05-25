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
        Guid SubscriptionPlanId,
        SubscriptionStatus SubscriptionStatus,
        int? MaxUsersOverride);

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
                request.SubscriptionPlanId,
                request.SubscriptionStatus,
                request.MaxUsersOverride);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
