namespace Application.Billing.InitiatePayment;

public sealed record InitiatePaymentResponse
{
    public Guid PaymentId { get; init; }
    public string Qr { get; init; }
    public string Md5 { get; init; }
}
