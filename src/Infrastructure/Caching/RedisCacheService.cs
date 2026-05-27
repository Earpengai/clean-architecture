using System.Text.Json;
using Application.Abstractions.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Caching;

internal sealed class RedisCacheService : ICacheService
{
    private const string KeyPrefix = "cache:";

    private readonly IDatabase _database;
    private readonly CacheOptions _options;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        IOptions<CacheOptions> options,
        ILogger<RedisCacheService> logger)
    {
        _database = redis.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            RedisValue value = await _database.StringGetAsync(PrefixedKey(key));

            if (!value.HasValue)
            {
                return default;
            }

            T? result = JsonSerializer.Deserialize<T>((string)value!);

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to get cached value for key '{Key}'", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            string serialized = JsonSerializer.Serialize(value);
            TimeSpan expiry = expiration ?? _options.DefaultExpiration;

            await _database.StringSetAsync(PrefixedKey(key), serialized, expiry);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to set cached value for key '{Key}'", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await _database.KeyDeleteAsync(PrefixedKey(key));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to remove cached value for key '{Key}'", key);
        }
    }

    private static string PrefixedKey(string key)
    {
        return KeyPrefix + key;
    }
}
