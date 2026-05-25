using Application.Abstractions.Messaging;
using Application.Admin.UpdateSubscriptionStatus;
using Domain.Tenants;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class UpdateSubscriptionStatus : IEndpoint
{
    public sealed record Request(SubscriptionStatus NewStatus);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/subscriptions/{subscriptionId:guid}/status", async (
            Guid subscriptionId,
            Request request,
            ICommandHandler<UpdateSubscriptionStatusCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSubscriptionStatusCommand(subscriptionId, request.NewStatus);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
