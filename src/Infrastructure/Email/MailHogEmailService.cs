using Application.Abstractions.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Email;

internal sealed class MailHogEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            configuration["Smtp:SenderName"],
            configuration["Smtp:SenderEmail"]!));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = htmlBody
        };

        string host = configuration["Smtp:Host"]!;
        int port = configuration.GetValue<int>("Smtp:Port");
        bool enableSsl = configuration.GetValue<bool>("Smtp:EnableSsl");

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
