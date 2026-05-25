using Application.Abstractions.Messaging;
using Application.SubscriptionFeatures.UpdatePlanLimit;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class UpdatePlanLimit : IEndpoint
{
    public sealed record Request(int Value);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/subscription/plans/{planId:guid}/limits/{limit}", async (
            Guid planId,
            string limit,
            Request request,
            ICommandHandler<UpdatePlanLimitCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdatePlanLimitCommand(planId, limit, request.Value);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
