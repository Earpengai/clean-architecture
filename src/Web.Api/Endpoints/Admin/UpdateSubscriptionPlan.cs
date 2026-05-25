using Application.Abstractions.Messaging;
using Application.Admin.UpdateSubscriptionPlan;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class UpdateSubscriptionPlan : IEndpoint
{
    public sealed record Request(
        string Name,
        string? Description,
        decimal PriceMonthly,
        decimal PriceYearly,
        int TrialDays,
        int SortOrder,
        bool IsActive);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("admin/subscription/plans/{planId:guid}", async (
            Guid planId,
            Request request,
            ICommandHandler<UpdateSubscriptionPlanCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSubscriptionPlanCommand(
                planId,
                request.Name,
                request.Description,
                request.PriceMonthly,
                request.PriceYearly,
                request.TrialDays,
                request.SortOrder,
                request.IsActive);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
