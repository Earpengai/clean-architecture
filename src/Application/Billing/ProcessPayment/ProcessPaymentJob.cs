using SharedKernel;

namespace Application.Billing.ProcessPayment;

public sealed record ProcessPaymentJob(Guid PaymentId, int Attempt = 0) : IBackgroundJob;
