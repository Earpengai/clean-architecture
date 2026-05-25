using Infrastructure.Email;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Jobs;

internal sealed class SendEmailJobHandler(
    MailHogEmailService mailHogEmailService,
    ILogger<SendEmailJobHandler> logger)
    : IBackgroundJobHandler<SendEmailJob>
{
    public async Task Handle(SendEmailJob job, CancellationToken cancellationToken)
    {
        logger.LogDebug("Sending email to '{Recipient}' with subject '{Subject}'", job.To, job.Subject);

        await mailHogEmailService.SendAsync(job.To, job.Subject, job.HtmlBody, cancellationToken);

        logger.LogDebug("Email sent to '{Recipient}'", job.To);
    }
}
