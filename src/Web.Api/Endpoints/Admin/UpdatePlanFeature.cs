using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.UpdatePlanFeature;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class UpdatePlanFeature : IEndpoint
{
    public sealed record Request(bool IsEnabled);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/subscription/plans/{planId:guid}/features/{feature}", async (
            Guid planId,
            string feature,
            Request request,
            ICommandHandler<UpdatePlanFeatureCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePlanFeatureCommand(planId, feature, request.IsEnabled);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
