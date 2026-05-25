using SharedKernel;

namespace Application.Abstractions.Jobs;

public interface IBackgroundJobQueue
{
    Task EnqueueAsync<T>(T job, CancellationToken cancellationToken = default) where T : IBackgroundJob;
}
