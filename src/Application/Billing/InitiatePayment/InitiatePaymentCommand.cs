using Application.Abstractions.Messaging;
using Domain.Tenants;

namespace Application.Billing.InitiatePayment;

public sealed record InitiatePaymentCommand(
    SubscriptionPlan Plan,
    SubscriptionBillingPeriod BillingPeriod) : ICommand<InitiatePaymentResponse>;
