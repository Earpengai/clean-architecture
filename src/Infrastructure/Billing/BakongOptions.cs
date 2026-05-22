namespace Infrastructure.Billing;

internal sealed class BakongOptions
{
    public const string Section = "Bakong";

    public string BaseUrl { get; init; } = "https://sit-api-bakong.nbc.gov.kh";
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string BakongAccountId { get; init; } = string.Empty;
}
