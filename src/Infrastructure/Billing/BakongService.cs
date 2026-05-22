using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Billing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Billing;

internal sealed class BakongService(
    IHttpClientFactory httpClientFactory,
    IOptions<BakongOptions> bakongOptions,
    ILogger<BakongService> logger) : IBakongService
{
    private readonly BakongOptions _options = bakongOptions.Value;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public Task<QrGenerationResult> GenerateQrAsync(
        BakongGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var individualInfo = new kh.gov.nbc.bakong_khqr.model.IndividualInfo
        {
            BakongAccountID = _options.BakongAccountId,
            Currency = request.Currency == "USD"
                ? kh.gov.nbc.bakong_khqr.model.KHQRCurrency.USD
                : kh.gov.nbc.bakong_khqr.model.KHQRCurrency.KHR,
            Amount = (double)request.Amount,
            MerchantName = "Bakong Merchant",
            MerchantCity = "PHNOM PENH",
            ExpirationTimestamp = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeMilliseconds(),
            BillNumber = Guid.NewGuid().ToString("N")[..10],
            StoreLabel = "STORE",
            TerminalLabel = "TERMINAL1",
            MobileNumber = "012345678",
            PurposeOfTransaction = "Payment"
        };

        kh.gov.nbc.bakong_khqr.model.KHQRResponse<kh.gov.nbc.bakong_khqr.model.KHQRData>? result =
            kh.gov.nbc.bakong_khqr.BakongKHQR.GenerateIndividual(individualInfo);

        if (result is null || result.Status.Code != 0)
        {
            string message = result?.Status.Message ?? "Unknown error";
            logger.LogError("KHQR generation failed: {Message}", message);
            throw new InvalidOperationException($"Failed to generate KHQR payment: {message}");
        }

        return Task.FromResult(new QrGenerationResult
        {
            Qr = result.Data.QR,
            Md5 = result.Data.MD5
        });
    }

    public async Task<TransactionCheckResult?> CheckTransactionAsync(
        string md5,
        CancellationToken cancellationToken = default)
    {
        using HttpClient client = httpClientFactory.CreateClient();
        string token = await GetTokenAsync(client, cancellationToken);

        var payload = new { md5 };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v1/check_transaction_by_md5")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        try
        {
            HttpResponseMessage response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Bakong check transaction returned {StatusCode} for md5 {Md5}",
                    (int)response.StatusCode,
                    md5);
                return null;
            }

            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var doc = JsonDocument.Parse(responseBody);
            JsonElement root = doc.RootElement;

            int responseCode = root.GetProperty("responseCode").GetInt32();

            if (responseCode != 0)
            {
                return null;
            }

            JsonElement data = root.GetProperty("data");

            return new TransactionCheckResult
            {
                Hash = data.GetProperty("hash").GetString() ?? string.Empty,
                FromAccountId = data.GetProperty("fromAccountId").GetString() ?? string.Empty,
                ToAccountId = data.GetProperty("toAccountId").GetString() ?? string.Empty,
                Currency = data.GetProperty("currency").GetString() ?? string.Empty,
                Amount = data.GetProperty("amount").GetDecimal(),
                Description = data.TryGetProperty("description", out JsonElement desc) && desc.ValueKind != JsonValueKind.Null
                    ? desc.GetString() ?? string.Empty
                    : string.Empty,
                CreatedDateMs = data.GetProperty("createdDateMs").GetInt64(),
                AcknowledgedDateMs = data.GetProperty("acknowledgedDateMs").GetInt64(),
                ExternalRef = data.GetProperty("externalRef").GetString() ?? string.Empty
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Bakong check transaction request failed for md5 {Md5}", md5);
            return null;
        }
    }

    private async Task<string> GetTokenAsync(HttpClient client, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_options.Token))
        {
            return _options.Token;
        }

        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
        {
            return _cachedToken;
        }

        var payload = new { email = _options.Email };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.BaseUrl}/v1/renew_token")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json")
        };

        HttpResponseMessage response = await client.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = JsonDocument.Parse(responseBody);
        JsonElement root = doc.RootElement;

        string token = root.GetProperty("data").GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Token not found in response");

        JwtSecurityToken jwt = new(token);
        _tokenExpiry = jwt.ValidTo;
        _cachedToken = token;

        return token;
    }
}

