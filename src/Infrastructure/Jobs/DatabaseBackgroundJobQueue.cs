using System.Text.Json;
using Application.Abstractions.Jobs;
using Infrastructure.Database;
using Microsoft.Extensions.Logging;
using SharedKernel;
using StackExchange.Redis;

namespace Infrastructure.Jobs;

internal sealed class DatabaseBackgroundJobQueue(
    ApplicationDbContext context,
    IConnectionMultiplexer redis,
    ILogger<DatabaseBackgroundJobQueue> logger)
    : IBackgroundJobQueue
{
    public async Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default)
        where T : IBackgroundJob
    {
        string jobType = typeof(T).AssemblyQualifiedName!;
        string payload = JsonSerializer.Serialize(job);

        var backgroundJob = new BackgroundJob
        {
            JobType = jobType,
            Payload = payload
        };

        context.Set<BackgroundJob>().Add(backgroundJob);
        await context.SaveChangesAsync(cancellationToken);

        try
        {
            ISubscriber subscriber = redis.GetSubscriber();
            await subscriber.PublishAsync(RedisChannel.Literal("jobs:notifications"), string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish Redis notification for background job {JobId}", backgroundJob.Id);
        }
    }
}
