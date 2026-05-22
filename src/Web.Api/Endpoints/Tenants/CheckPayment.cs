using Application.Abstractions.Messaging;
using Application.Billing.CheckPayment;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Tenants;

internal sealed class CheckPayment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("tenant/subscription/payment/{md5}", async (
            string md5,
            IQueryHandler<CheckPaymentQuery, CheckPaymentResponse> handler,
            CancellationToken cancellationToken) =>
        {
            CheckPaymentQuery query = new(md5);

            Result<CheckPaymentResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(Tags.Subscription);
    }
}
