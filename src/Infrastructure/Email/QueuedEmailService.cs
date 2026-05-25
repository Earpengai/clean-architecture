using Application.Abstractions.Email;
using Application.Abstractions.Jobs;
using Infrastructure.Jobs;

namespace Infrastructure.Email;

internal sealed class QueuedEmailService(IBackgroundJobQueue jobQueue) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var job = new SendEmailJob(to, subject, htmlBody);
        await jobQueue.EnqueueAsync(job, cancellationToken);
    }
}
