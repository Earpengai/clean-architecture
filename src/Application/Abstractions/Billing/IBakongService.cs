namespace Application.Abstractions.Billing;

public sealed record BakongGenerationRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    public string MerchantName { get; init; }
}

public sealed record QrGenerationResult
{
    public string Qr { get; init; }
    public string Md5 { get; init; }
}

public sealed record TransactionCheckResult
{
    public string Hash { get; init; }
    public string FromAccountId { get; init; }
    public string ToAccountId { get; init; }
    public string Currency { get; init; }
    public decimal Amount { get; init; }
    public string Description { get; init; }
    public long CreatedDateMs { get; init; }
    public long AcknowledgedDateMs { get; init; }
    public string ExternalRef { get; init; }
}

public interface IBakongService
{
    Task<QrGenerationResult> GenerateQrAsync(BakongGenerationRequest request, CancellationToken cancellationToken = default);

    Task<TransactionCheckResult?> CheckTransactionAsync(string md5, CancellationToken cancellationToken = default);
}
