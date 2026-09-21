using Microsoft.AspNetCore.Identity.UI.Services;

namespace BookDriver.Services;

// Demo-only sender: logs instead of actually sending, since no SMTP provider is configured.
public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("Email suppressed (no SMTP configured). To: {Email}, Subject: {Subject}", email, subject);
        return Task.CompletedTask;
    }
}
