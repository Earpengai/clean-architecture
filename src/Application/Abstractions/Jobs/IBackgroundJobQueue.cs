using SharedKernel;

namespace Application.Abstractions.Jobs;

public interface IBackgroundJobQueue
{
    Task EnqueueAsync<T>(
        T job,
        DateTime? scheduledAt = null,
        int? maxRetries = null,
        CancellationToken cancellationToken = default)
        where T : IBackgroundJob;
}
