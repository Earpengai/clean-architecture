namespace Infrastructure.Jobs;

public sealed class BackgroundJob
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string JobType { get; init; } = string.Empty;

    public string Payload { get; init; } = string.Empty;

    public BackgroundJobStatus Status { get; set; } = BackgroundJobStatus.Pending;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? ScheduledAt { get; init; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Error { get; set; }

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; } = 3;

    public Guid? TenantId { get; init; }
}
