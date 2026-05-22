using SharedKernel;

namespace Domain.Payments;

public static class PaymentErrors
{
    public static Error NotFound(Guid paymentId) => Error.NotFound(
        "Payments.NotFound",
        $"The payment with the Id = '{paymentId}' was not found");

    public static readonly Error NotOwner = Error.Failure(
        "Payments.NotOwner",
        "Only the tenant owner can initiate a payment");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        "Payments.AlreadyCompleted",
        "This payment has already been processed");

    public static Error NotFoundByMd5(string md5) => Error.NotFound(
        "Payments.NotFoundByMd5",
        $"No pending payment found for MD5 hash '{md5}'");
}
