# Background Job Queue

DB-backed outbox pattern for asynchronous background jobs with Redis Pub/Sub notifications for immediate pickup. Jobs are persisted in PostgreSQL, processed by a `BackgroundService`, with automatic retry on failure.

---

## Architecture

```
HTTP Request → IBackgroundJobQueue.EnqueueAsync()
  → DatabaseBackgroundJobQueue
    → INSERT INTO background_jobs (durable, survives restart)
    → PUBLISH "jobs:notifications" (Redis, wake-up signal)

BackgroundJobProcessor (BackgroundService):
  → SUBSCRIBE "jobs:notifications" (Redis, instant wake)
  → poll DB every 10 seconds (fallback if Redis is down)
  → SELECT WHERE status = 'Pending' ORDER BY created_at
  → Resolve IBackgroundJobHandler<T> from DI (cached reflection)
  → handler.Handle(job)
  → UPDATE status = Completed | Failed
  → retry up to 3 times on failure
```

### Data flow across layers

```
Web.Api / Application        Infrastructure
─────────────────────────    ──────────────────────────────
IEmailService.SendAsync()    → QueuedEmailService
                               → IBackgroundJobQueue.EnqueueAsync()
                                 → DatabaseBackgroundJobQueue
                                   → ApplicationDbContext.SaveChangesAsync()
                                   → Redis PUBLISH
                                 
                             BackgroundJobProcessor (runs independently)
                               → ApplicationDbContext (poll)
                               → IBackgroundJobHandler<T> (resolve + execute)
```

---

## Abstractions

| Interface | Layer | Purpose |
|-----------|-------|---------|
| `IBackgroundJob` | SharedKernel | Marker interface for job payloads |
| `IBackgroundJobHandler<T>` | SharedKernel | Handler contract — `Task Handle(T, CancellationToken)` |
| `IBackgroundJobQueue` | Application | Enqueue contract — `Task EnqueueAsync<T>(T, CancellationToken)` |

### `IBackgroundJob`

```csharp
// src/SharedKernel/IBackgroundJob.cs
namespace SharedKernel;

public interface IBackgroundJob;
```

### `IBackgroundJobHandler<T>`

```csharp
// src/SharedKernel/IBackgroundJobHandler.cs
namespace SharedKernel;

public interface IBackgroundJobHandler<in T> where T : IBackgroundJob
{
    Task Handle(T job, CancellationToken cancellationToken);
}
```

### `IBackgroundJobQueue`

```csharp
// src/Application/Abstractions/Jobs/IBackgroundJobQueue.cs
namespace Application.Abstractions.Jobs;

public interface IBackgroundJobQueue
{
    Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default)
        where T : IBackgroundJob;
}
```

---

## BackgroundJob Entity

Persisted in the `background_jobs` table (schema `public`).

| Column | Type | Purpose |
|--------|------|---------|
| `id` | `uuid` PK | Job identifier |
| `job_type` | `text` (max 500) | `AssemblyQualifiedName` for deserialization |
| `payload` | `text` (JSON) | Serialized job data |
| `status` | `text` (max 50) | `Pending` / `Processing` / `Completed` / `Failed` |
| `created_at` | `timestamptz` | When enqueued |
| `scheduled_at` | `timestamptz?` | Delayed execution (null = immediate) |
| `started_at` | `timestamptz?` | When processing began |
| `completed_at` | `timestamptz?` | When finished |
| `error` | `text?` | Failure message |
| `retry_count` | `int` | Attempts so far |
| `max_retries` | `int` | Max attempts (default 3) |
| `tenant_id` | `uuid?` | Multi-tenant scope (auto-populated by `TenantSaveInterceptor`) |

### Entity configuration

```csharp
// src/Infrastructure/Jobs/BackgroundJobConfiguration.cs
internal sealed class BackgroundJobConfiguration : IEntityTypeConfiguration<BackgroundJob>
{
    public void Configure(EntityTypeBuilder<BackgroundJob> builder)
    {
        builder.ToTable("background_jobs");
        builder.HasKey(j => j.Id);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.CreatedAt);
        builder.HasIndex(j => j.ScheduledAt);
        builder.Property(j => j.JobType).HasMaxLength(500).IsRequired();
        builder.Property(j => j.Payload).IsRequired();
        builder.Property(j => j.Status).HasConversion<string>().HasMaxLength(50);
    }
}
```

Indexes on `Status`, `CreatedAt`, and `ScheduledAt` optimize the processor's polling query.

---

## Enqueue Flow

When `EnqueueAsync<T>` is called:

1. Serialize `T` to JSON via `System.Text.Json`
2. Store `AssemblyQualifiedName` as `JobType` (e.g., `"Infrastructure.Jobs.SendEmailJob, Infrastructure"`)
3. Insert `BackgroundJob` entity into `ApplicationDbContext`
4. Call `SaveChangesAsync` — persists to PostgreSQL immediately
5. Publish Redis notification on channel `"jobs:notifications"` (fire-and-forget)
6. On Redis failure: log warning, continue — processor will pick up via polling

```csharp
// src/Infrastructure/Jobs/DatabaseBackgroundJobQueue.cs (simplified)
public async Task EnqueueAsync<T>(T job, CancellationToken cancellationToken)
{
    string jobType = typeof(T).AssemblyQualifiedName!;
    string payload = JsonSerializer.Serialize(job);

    var backgroundJob = new BackgroundJob { JobType = jobType, Payload = payload };
    context.Set<BackgroundJob>().Add(backgroundJob);
    await context.SaveChangesAsync(cancellationToken);

    try
    {
        await redis.GetSubscriber()
            .PublishAsync(RedisChannel.Literal("jobs:notifications"), string.Empty);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to publish Redis notification");
    }
}
```

---

## Processing Flow

The `BackgroundJobProcessor` (`BackgroundService`) runs continuously:

### Startup

1. `ExecuteAsync` fires — processes any pending jobs left from previous runs
2. Starts two concurrent loops: Redis subscription + periodic polling

### Redis subscription loop

```csharp
ISubscriber subscriber = redis.GetSubscriber();
ChannelMessageQueue queue = await subscriber.SubscribeAsync(
    RedisChannel.Literal("jobs:notifications"));

await foreach (ChannelMessage _ in queue.WithCancellation(stoppingToken))
{
    await ProcessPendingJobsAsync(stoppingToken);
}
```

### Polling loop (fallback)

```csharp
using PeriodicTimer timer = new(TimeSpan.FromSeconds(10));
while (await timer.WaitForNextTickAsync(stoppingToken))
{
    await ProcessPendingJobsAsync(stoppingToken);
}
```

### Job dispatch (cached reflection)

Uses the same pattern as `DomainEventsDispatcher` — a `ConcurrentDictionary` caches `typeof(JobWrapper<>)` open-generic types:

```csharp
var jobType = Type.GetType(backgroundJob.JobType);
var wrapper = JobWrapper.Create(jobType);          // resolves JobWrapper<T> via cached reflection
object? deserializedJob = JsonSerializer.Deserialize(payload, jobType);

await wrapper.ExecuteAsync(serviceProvider, deserializedJob, cancellationToken);
// internally resolves IBackgroundJobHandler<T> from DI and calls Handle()
```

---

## Retry & Error Handling

| Attempt | Action |
|---------|--------|
| Handler throws | `RetryCount++`, status set to `Pending` (retry on next poll) |
| RetryCount >= MaxRetries | Status set to `Failed`, error message persisted |
| Job type not found | Status set to `Failed` immediately (no retry) |
| Payload deserialization fails | Status set to `Failed` immediately (no retry) |

All failures are logged via Serilog at `Warning` (retry) or `Error` (final failure) level. The processor survives individual job failures — a failing job does not crash the processor or block other jobs.

---

## Adding a New Job Type

### Step 1: Define the job record

```csharp
// src/Infrastructure/Jobs/GenerateReportJob.cs
using SharedKernel;

namespace Infrastructure.Jobs;

internal sealed record GenerateReportJob(
    Guid TenantId,
    Guid ReportId,
    string ReportType) : IBackgroundJob;
```

### Step 2: Implement the handler

```csharp
// src/Infrastructure/Jobs/GenerateReportJobHandler.cs
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Jobs;

internal sealed class GenerateReportJobHandler(
    ILogger<GenerateReportJobHandler> logger)
    : IBackgroundJobHandler<GenerateReportJob>
{
    public async Task Handle(GenerateReportJob job, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating {ReportType} report {ReportId} for tenant {TenantId}",
            job.ReportType, job.ReportId, job.TenantId);

        // ... long-running report generation ...

        logger.LogInformation("Report {ReportId} generated", job.ReportId);
    }
}
```

### Step 3: Register in DI

```csharp
// src/Infrastructure/DependencyInjection.cs → AddServices()
services.AddScoped<IBackgroundJobHandler<GenerateReportJob>, GenerateReportJobHandler>();
```

The `BackgroundJobProcessor` auto-discovers handlers via `GetServices<IBackgroundJobHandler<T>>()` — no changes to the processor itself.

### Step 4: Enqueue from any application code

```csharp
// In a command handler, domain event handler, or endpoint:
await jobQueue.EnqueueAsync(new GenerateReportJob(tenantId, reportId, "Monthly"), cancellationToken);
```

---

## Email Integration Example

Email is the primary use case shipped with the template. The integration is transparent — domain event handlers call `IEmailService.SendAsync()` without knowing about the queue.

### Wiring

```
IEmailService → QueuedEmailService → IBackgroundJobQueue.EnqueueAsync(SendEmailJob)
                                                        ↓
BackgroundJobProcessor → IBackgroundJobHandler<SendEmailJob> → SendEmailJobHandler
                                                                  ↓
                                                                MailHogEmailService (SMTP via MailKit)
```

### Registration

```csharp
// Core: domain event handlers call IEmailService (resolves to QueuedEmailService)
services.AddScoped<IEmailService, QueuedEmailService>();

// Raw SMTP: used only by SendEmailJobHandler in the background process
services.AddScoped<MailHogEmailService>();

// Wire the job type to its handler
services.AddScoped<IBackgroundJobHandler<SendEmailJob>, SendEmailJobHandler>();
```

### SendEmailJobHandler

Injects `MailHogEmailService` directly (not `IEmailService`) to avoid re-enqueueing emails:

```csharp
internal sealed class SendEmailJobHandler(
    MailHogEmailService mailHogEmailService,
    ILogger<SendEmailJobHandler> logger)
    : IBackgroundJobHandler<SendEmailJob>
{
    public async Task Handle(SendEmailJob job, CancellationToken cancellationToken)
    {
        await mailHogEmailService.SendAsync(job.To, job.Subject, job.HtmlBody, cancellationToken);
    }
}
```

---

## Configuration

### Redis connection string

```jsonc
// appsettings.Development.json
{
  "Redis": {
    "ConnectionString": "redis:6379"
  }
}
```

The `AbortOnConnectFail = false` option ensures the application starts even if Redis is temporarily unavailable.

### Polling interval

Hard-coded in `BackgroundJobProcessor`:

```csharp
private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);
```

### Retry settings

Default `MaxRetries = 3` on the `BackgroundJob` entity. Can be overridden per job by setting `MaxRetries` before enqueue.

### Docker Compose

```yaml
redis:
  image: redis:7-alpine
  container_name: redis
  ports:
    - 6379:6379
  volumes:
    - ./.containers/redis:/data
```

---

## Gotchas

- **Type name stability**: Jobs are stored with `AssemblyQualifiedName`. Renaming or moving a job class breaks pending jobs in the queue (they will fail with "unknown type" errors). Always drain the queue before renaming job types.

- **Single-instance safe**: The processor uses `.Take(5)` without `SKIP LOCKED`. This is safe for a single application instance. For multi-instance deployments, add `FOR UPDATE SKIP LOCKED` to prevent duplicate processing.

- **No migration yet**: The `background_jobs` table is created via `EnsureCreated()` in development. EF migrations tool (`dotnet-ef`) has a known compatibility issue with .NET 10 preview SDK. A migration can be added when the tool issue is resolved.

- **Redis notification is best-effort**: Redis publish failures are logged as warnings, not thrown. The polling loop guarantees eventual delivery even if Redis is completely unavailable.

- **Handler resolution uses the job scope**: Handlers are resolved from the same `IServiceScope` as the DB context. Scoped dependencies (e.g., `IApplicationDbContext`) work correctly. Do not resolve handlers from the root provider.

- **No delayed execution**: `ScheduledAt` column exists but is not yet wired to support scheduling. All jobs run as soon as the processor finds them.

- **Circular dependency avoided**: `SendEmailJobHandler` injects `MailHogEmailService` directly (concrete type), not `IEmailService`. Injecting `IEmailService` would resolve to `QueuedEmailService` and create an infinite enqueue loop.
