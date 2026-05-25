using SharedKernel;

namespace Infrastructure.Jobs;

internal sealed record SendEmailJob(string To, string Subject, string HtmlBody) : IBackgroundJob;
