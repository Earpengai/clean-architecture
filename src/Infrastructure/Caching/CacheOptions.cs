namespace Infrastructure.Caching;

internal sealed class CacheOptions
{
    public const string Section = "Cache";

    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);
}
