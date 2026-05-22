using Application.Abstractions.Messaging;

namespace Application.Billing.CheckPayment;

public sealed record CheckPaymentQuery(string Md5) : IQuery<CheckPaymentResponse>;
