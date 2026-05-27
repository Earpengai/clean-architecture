# Distributed Caching

Redis-backed cache-aside layer that eliminates redundant database queries on the authorization hot path. Reuses the existing `IConnectionMultiplexer` singleton — no new NuGet packages required. Cache failures are non-critical: logged as warnings and bypassed so the application degrades gracefully.

---

## Architecture

```
Request → PermissionAuthorizationHandler / SubscriptionFeatureAuthorizationHandler
  → PermissionProvider / SubscriptionFeatureProvider
    → ICacheService.GetAsync(key)          ← Redis STRING GET
      → hit  → return cached (skip DB)
      → miss → DB query → ICacheService.SetAsync(key, value) → Redis STRING SET → return
```

### Layer Map

| Layer          | File                                                                 | Role                    |
|----------------|----------------------------------------------------------------------|-------------------------|
| Application    | `src/Application/Abstractions/Caching/ICacheService.cs`              | Interface (public)      |
| Infrastructure | `src/Infrastructure/Caching/RedisCacheService.cs`                    | Implementation          |
| Infrastructure | `src/Infrastructure/Caching/CacheOptions.cs`                         | Configuration options   |
| Infrastructure | `src/Infrastructure/DependencyInjection.cs`                          | DI registration         |

---

## ICacheService

Namespace: `Application.Abstractions.Caching`

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
```

- **`GetAsync<T>`** — returns `default` (null) on cache miss or Redis failure. Always use nullable types for `T` so you can distinguish miss from hit.
- **`SetAsync<T>`** — writes the value with an optional expiration. Falls back to `CacheOptions.DefaultExpiration` (5 min) when not specified.
- **`RemoveAsync`** — deletes a single key. Use for explicit invalidation after domain mutations.

---

## Cache Keys

All keys are namespaced under the `"cache:"` prefix in Redis to avoid collisions with the background job pub/sub channel.

| Key Pattern                | Cached By                  | Cached Value       | TTL     |
|----------------------------|----------------------------|--------------------|---------|
| `permissions:{userId}`     | `PermissionProvider`       | `HashSet<string>`  | 5 min   |
| `features:{tenantId}`      | `SubscriptionFeatureProvider` | `HashSet<string>` | 5 min   |
| `limits:{tenantId}:{limitKey}` | `SubscriptionFeatureProvider` | `int?`           | 5 min   |

---

## RedisCacheService

Namespace: `Infrastructure.Caching`

```csharp
internal sealed class RedisCacheService : ICacheService
```

Implementation details:

- Uses `IDatabase` from the singleton `IConnectionMultiplexer` — same connection the background job queue uses for pub/sub.
- Serialization: `System.Text.Json`. Generic types must be JSON-serializable.
- Key prefix: every key is stored as `"cache:" + key` in Redis.
- Cancellation: `ThrowIfCancellationRequested` is checked at method entry. `OperationCanceledException` is not swallowed by the catch block — it propagates to the caller.
- All other exceptions (Redis unreachable, deserialization errors) are caught, logged at `LogWarning`, and the method returns `default` — the caller falls through to the database.

---

## Cached Providers

### PermissionProvider

Namespace: `Infrastructure.Authorization`

Every authorized request runs `PermissionAuthorizationHandler` → `PermissionProvider.GetForUserIdAsync(userId)`. This was a database query on every request. Caching eliminates it after the first miss.

```csharp
public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
{
    string cacheKey = $"permissions:{userId}";

    HashSet<string>? cached = await cacheService.GetAsync<HashSet<string>>(cacheKey);

    if (cached is not null)
    {
        return cached;               // cache hit — no DB
    }

    // cache miss — query DB
    using IServiceScope scope = serviceScopeFactory.CreateScope();
    ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    List<string> permissions = await context.Memberships
        .Where(m => m.UserId == userId)
        .SelectMany(m => m.Role.Permissions)
        .Select(rp => rp.Permission)
        .Distinct()
        .ToListAsync();

    HashSet<string> result = [.. permissions];

    await cacheService.SetAsync(cacheKey, result);

    return result;
}
```

### SubscriptionFeatureProvider

Namespace: `Infrastructure.SubscriptionFeatures`

Two methods are cached:

- **`GetEnabledFeaturesAsync(tenantId)`** — caches `HashSet<string>` of enabled feature names. An empty `HashSet` is cached when the tenant has no subscription plan, avoiding repeated DB queries.
- **`GetLimitAsync(tenantId, limitKey)`** — caches `int?` limit values. Null values are NOT cached (the `cached is not null` check would never match), so limits that don't exist in the plan are re-queried on each request. This is intentional — a limit might be added later.

---

## Configuration

### CacheOptions

Namespace: `Infrastructure.Caching`

```csharp
internal sealed class CacheOptions
{
    public const string Section = "Cache";

    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(5);
}
```

### appsettings.json

Add a `"Cache"` section to override defaults:

```json
{
    "Cache": {
        "DefaultExpiration": "00:05:00"
    }
}
```

If omitted, the default is 5 minutes.

### Per-Call Override

The `expiration` parameter on `SetAsync` overrides the default:

```csharp
await cacheService.SetAsync(key, value, expiration: TimeSpan.FromMinutes(10));
```

### DI Registration

In `src/Infrastructure/DependencyInjection.cs`:

```csharp
services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.Section));
services.AddSingleton<ICacheService, RedisCacheService>();
```

`ICacheService` is a singleton (thread-safe) and can be injected into scoped and transient services freely.

---

## Using ICacheService in a Query Handler

Add `ICacheService` as a constructor dependency, then apply the cache-aside pattern:

```csharp
internal sealed class GetTenantByIdQueryHandler(
    IApplicationDbContext context,
    ICacheService cacheService)
    : IQueryHandler<GetTenantByIdQuery, TenantResponse>
{
    public async Task<Result<TenantResponse>> Handle(
        GetTenantByIdQuery query,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"tenant:{query.TenantId}";

        TenantResponse? cached = await cacheService.GetAsync<TenantResponse>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        TenantResponse? tenant = await context.Tenants
            .Where(t => t.Id == query.TenantId)
            .Select(t => new TenantResponse(t.Id, t.Name))
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantResponse>(TenantErrors.NotFound(query.TenantId));
        }

        await cacheService.SetAsync(cacheKey, tenant, cancellationToken: cancellationToken);

        return tenant;
    }
}
```

### Key Naming Convention

Use a consistent prefix that identifies the entity: `tenant:{id}`, `user:{id}`, `todo:{id}`. This makes keys self-documenting and simplifies invalidation.

---

## Cache Invalidation

### TTL-Based (Primary Strategy)

The 5-minute default expiration is the primary invalidation mechanism. It bounds staleness without requiring explicit invalidation hooks in every mutation path. This is simple, resilient, and covers all cases — even edge cases where a mutation doesn't have an invalidation hook.

### Explicit Invalidation

Call `RemoveAsync` after domain mutations that change cached data. Example in a role assignment command handler:

```csharp
internal sealed class AssignRoleCommandHandler(
    IApplicationDbContext context,
    ICacheService cacheService)
    : ICommandHandler<AssignRoleCommand>
{
    public async Task<Result> Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        // ... assign role, save changes ...

        await cacheService.RemoveAsync($"permissions:{command.UserId}", cancellationToken);

        return Result.Success();
    }
}
```

Invalidate immediately, don't wait for TTL expiration. The cache will repopulate on the next request.

- **Permission changes** (role assigned, permission added/removed) → invalidate `permissions:{userId}`
- **Subscription changes** (plan changed, feature toggled) → invalidate `features:{tenantId}` and relevant `limits:{tenantId}:*` keys
- **Entity updates** → invalidate `entity:{id}`

---

## Failure Handling

Redis is treated as non-critical infrastructure:

```
Redis down → ConnectionMultiplexer fails → Exception thrown
  → catch (Exception ex) when (ex is not OperationCanceledException)
  → _logger.LogWarning("Failed to get/set/remove cached value for key '{Key}'", key)
  → GetAsync returns default (null) → caller falls through to DB
  → SetAsync/RemoveAsync silently no-ops
```

The application continues serving requests without caching. When Redis recovers, `StackExchange.Redis` auto-reconnects and caching resumes transparently.

If Redis is down and traffic is high, warning logs will be emitted on every request. This is intentional — the volume signals that Redis needs attention while the application remains available.

### Health Check

Redis connectivity is already monitored by the health check endpoint:

```csharp
// src/Infrastructure/DependencyInjection.cs
.AddRedis(configuration["Redis:ConnectionString"] ?? "localhost:6379");
```

---

## Anti-Patterns

### Don't cache null and expect it to prevent DB queries

```csharp
// WRONG — cached is null on miss, so cached is not null is always false
int? cached = await cacheService.GetAsync<int?>(key, ct);
if (cached is not null) { return cached; }  // will never match a cached null
```

`GetAsync<T>` returns `default` (null) on miss. The `cached is not null` guard filters out both genuine cache misses AND cached null values. Don't cache null — it wastes a Redis roundtrip with no benefit. If you need to cache "not found", use a sentinel type instead of null.

### Don't cache non-serializable types

`JsonSerializer.Serialize` must be able to handle the type. Records, DTOs, collections, and primitives work. Types with circular references, `Dictionary` with non-string keys, or non-public setters may fail.

### Don't pass CancellationToken.None explicitly

The default parameter value (`= default`) is cleaner and has the same effect. Only pass a token when you have one.

---

## When to Cache

| Worth Caching                                  | Not Worth Caching                          |
|------------------------------------------------|--------------------------------------------|
| Hot-path authorization checks (every request)  | User-initiated queries called rarely       |
| Data that changes infrequently                 | Data that changes on every write           |
| Small payloads (strings, GUIDs, ints)          | Large datasets better served from the DB   |
| Shared across many tenants/users               | Single-tenant, single-user private data    |

The authorization pipeline (`PermissionProvider`, `SubscriptionFeatureProvider`) is the primary caching target — it runs on every authenticated request and the data changes only when roles or subscriptions are modified.
