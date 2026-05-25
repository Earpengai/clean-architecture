using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.Billing.InitiatePayment;

public sealed record InitiatePaymentCommand(
    Guid SubscriptionPlanId,
    SubscriptionBillingPeriod BillingPeriod) : ICommand<InitiatePaymentResponse>;
