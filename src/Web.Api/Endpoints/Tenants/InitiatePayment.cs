using Application.Abstractions.Messaging;
using Application.Billing.InitiatePayment;
using Domain.Permissions;
using Domain.Tenants;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class InitiatePayment : IEndpoint
{
    public sealed record Request(
        SubscriptionPlan Plan,
        SubscriptionBillingPeriod BillingPeriod);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("tenant/subscription/payment", async (
            Request request,
            ICommandHandler<InitiatePaymentCommand, InitiatePaymentResponse> handler,
            CancellationToken cancellationToken) =>
        {
            InitiatePaymentCommand command = new(request.Plan, request.BillingPeriod);

            Result<InitiatePaymentResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .HasPermission(Permission.TenantsWrite)
        .WithTags(Tags.Subscription);
    }
}
