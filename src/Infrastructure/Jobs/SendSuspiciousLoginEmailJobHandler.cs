using System.Globalization;
using Application.Users.DomainEventHandlers;
using Infrastructure.Email;
using SharedKernel;

namespace Infrastructure.Jobs;

internal sealed class SendSuspiciousLoginEmailJobHandler(
    MailHogEmailService emailService) : IBackgroundJobHandler<SuspiciousLoginEmailJob>
{
    public async Task Handle(SuspiciousLoginEmailJob job, CancellationToken cancellationToken)
    {
        string timestamp = job.Timestamp.ToString("f", CultureInfo.InvariantCulture);

        if (job.IsFirstSession)
        {
            string body = $"""
                <h2>Welcome to Clean Architecture</h2>
                <p>Your first sign-in was from:</p>
                <ul>
                    <li><strong>Browser:</strong> {job.Browser}</li>
                    <li><strong>Operating System:</strong> {job.Os}</li>
                    <li><strong>IP Address:</strong> {job.IpAddress}</li>
                    <li><strong>Time:</strong> {timestamp} UTC</li>
                </ul>
                <p>If this wasn't you, please change your password immediately.</p>
                """;

            await emailService.SendAsync(job.Email, "Welcome — new sign-in detected", body, cancellationToken);
        }
        else
        {
            string body = $"""
                <h2>New sign-in detected</h2>
                <p>Your account was signed in from a new device or location:</p>
                <ul>
                    <li><strong>Browser:</strong> {job.Browser}</li>
                    <li><strong>Operating System:</strong> {job.Os}</li>
                    <li><strong>IP Address:</strong> {job.IpAddress}</li>
                    <li><strong>Time:</strong> {timestamp} UTC</li>
                </ul>
                <p>If this was you, no action is needed.</p>
                <p>If this wasn't you, please change your password immediately.</p>
                """;

            await emailService.SendAsync(job.Email, "New sign-in from a new device", body, cancellationToken);
        }
    }
}
