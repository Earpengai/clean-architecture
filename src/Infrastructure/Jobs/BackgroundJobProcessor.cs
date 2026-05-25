using System.Collections.Concurrent;
using System.Text.Json;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedKernel;
using StackExchange.Redis;

namespace Infrastructure.Jobs;

internal sealed class BackgroundJobProcessor(
    IServiceScopeFactory serviceScopeFactory,
    IConnectionMultiplexer redis,
    ILogger<BackgroundJobProcessor> logger)
    : BackgroundService
{
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessPendingJobsAsync(stoppingToken);

        Task pollingTask = PollingLoopAsync(stoppingToken);
        Task redisTask = RedisSubscriptionLoopAsync(stoppingToken);

        await Task.WhenAny(pollingTask, redisTask);

        if (!stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning("One of the background job loops exited unexpectedly");
        }
    }

    private async Task PollingLoopAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_pollingInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessPendingJobsAsync(stoppingToken);
        }
    }

    private async Task RedisSubscriptionLoopAsync(CancellationToken stoppingToken)
    {
        ISubscriber subscriber = redis.GetSubscriber();

        ChannelMessageQueue queue = await subscriber.SubscribeAsync(RedisChannel.Literal("jobs:notifications"));

        await foreach (ChannelMessage _ in queue.WithCancellation(stoppingToken))
        {
            await ProcessPendingJobsAsync(stoppingToken);
        }
    }

    private async Task ProcessPendingJobsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            List<BackgroundJob> pendingJobs = await context.Set<BackgroundJob>()
                .Where(j => j.Status == BackgroundJobStatus.Pending
                            && (j.ScheduledAt == null || j.ScheduledAt <= DateTime.UtcNow))
                .OrderBy(j => j.CreatedAt)
                .Take(5)
                .ToListAsync(cancellationToken);

            if (pendingJobs.Count == 0)
            {
                return;
            }

            foreach (BackgroundJob backgroundJob in pendingJobs)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await ProcessJobAsync(scope, backgroundJob, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing pending background jobs");
        }
    }

    private async Task ProcessJobAsync(
        IServiceScope scope,
        BackgroundJob backgroundJob,
        CancellationToken cancellationToken)
    {
        try
        {
            backgroundJob.Status = BackgroundJobStatus.Processing;
            backgroundJob.StartedAt = DateTime.UtcNow;

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.SaveChangesAsync(cancellationToken);

            var jobType = Type.GetType(backgroundJob.JobType);

            if (jobType is null)
            {
                backgroundJob.Status = BackgroundJobStatus.Failed;
                backgroundJob.CompletedAt = DateTime.UtcNow;
                backgroundJob.Error = $"Job type '{backgroundJob.JobType}' could not be resolved.";

                await context.SaveChangesAsync(cancellationToken);

                logger.LogError("Background job {JobId}: unknown type '{JobType}'", backgroundJob.Id, backgroundJob.JobType);
                return;
            }

            var wrapper = JobWrapper.Create(jobType);
            object? deserializedJob = JsonSerializer.Deserialize(backgroundJob.Payload, jobType);

            if (deserializedJob is null)
            {
                backgroundJob.Status = BackgroundJobStatus.Failed;
                backgroundJob.CompletedAt = DateTime.UtcNow;
                backgroundJob.Error = "Failed to deserialize job payload.";

                await context.SaveChangesAsync(cancellationToken);

                logger.LogError("Background job {JobId}: failed to deserialize payload", backgroundJob.Id);
                return;
            }

            await wrapper.ExecuteAsync(scope.ServiceProvider, deserializedJob, cancellationToken);

            backgroundJob.Status = BackgroundJobStatus.Completed;
            backgroundJob.CompletedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogDebug("Background job {JobId} ({JobType}) completed", backgroundJob.Id, backgroundJob.JobType);
        }
        catch (Exception ex)
        {
            backgroundJob.RetryCount++;
            backgroundJob.Error = ex.Message;
            backgroundJob.CompletedAt = DateTime.UtcNow;

            if (backgroundJob.RetryCount >= backgroundJob.MaxRetries)
            {
                backgroundJob.Status = BackgroundJobStatus.Failed;

                logger.LogError(ex, "Background job {JobId} ({JobType}) failed after {RetryCount} retries",
                    backgroundJob.Id, backgroundJob.JobType, backgroundJob.RetryCount);
            }
            else
            {
                backgroundJob.Status = BackgroundJobStatus.Pending;

                logger.LogWarning(ex, "Background job {JobId} ({JobType}) failed (attempt {RetryCount}/{MaxRetries}), will retry",
                    backgroundJob.Id, backgroundJob.JobType, backgroundJob.RetryCount, backgroundJob.MaxRetries);
            }

            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private abstract class JobWrapper
    {
        public abstract Task ExecuteAsync(
            IServiceProvider serviceProvider,
            object job,
            CancellationToken cancellationToken);

        public static JobWrapper Create(Type jobType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                jobType,
                jt => typeof(JobWrapper<>).MakeGenericType(jt));

            return (JobWrapper)Activator.CreateInstance(wrapperType)!;
        }
    }

    private sealed class JobWrapper<T> : JobWrapper where T : IBackgroundJob
    {
        public override async Task ExecuteAsync(
            IServiceProvider serviceProvider,
            object job,
            CancellationToken cancellationToken)
        {
            IEnumerable<IBackgroundJobHandler<T>> handlers = serviceProvider
                .GetServices<IBackgroundJobHandler<T>>();

            foreach (IBackgroundJobHandler<T> handler in handlers)
            {
                await handler.Handle((T)job, cancellationToken);
            }
        }
    }
}
