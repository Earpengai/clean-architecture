using Application.Abstractions.Messaging;

namespace Application.Billing.GetPaymentHistory;

public sealed record GetPaymentHistoryQuery : IQuery<List<PaymentResponse>>;
