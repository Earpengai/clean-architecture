namespace SharedKernel;

public interface IBackgroundJobHandler<in T> where T : IBackgroundJob
{
    Task Handle(T job, CancellationToken cancellationToken);
}
