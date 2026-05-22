using Application.Abstractions.Messaging;
using Application.Billing.GetPaymentHistory;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class GetPaymentHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenant/subscription/payment/history", async (
            IQueryHandler<GetPaymentHistoryQuery, List<PaymentResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            GetPaymentHistoryQuery query = new();

            Result<List<PaymentResponse>> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Subscription);
    }
}
