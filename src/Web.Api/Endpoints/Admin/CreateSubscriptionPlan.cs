using Application.Abstractions.Messaging;
using Application.Admin.CreateSubscriptionPlan;
using Finbuckle.MultiTenant;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

internal sealed class CreateSubscriptionPlan : IEndpoint
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
        app.MapPost("admin/subscription/plans", async (
            Request request,
            ICommandHandler<CreateSubscriptionPlanCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSubscriptionPlanCommand(
                request.Name,
                request.Description,
                request.PriceMonthly,
                request.PriceYearly,
                request.TrialDays,
                request.SortOrder,
                request.IsActive);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(
                value => Results.Created($"/admin/subscription/plans/{value}", value),
                CustomResults.Problem);
        })
        .RequireAuthorization()
        .ExcludeFromMultiTenantResolution()
        .WithTags("Admin");
    }
}
