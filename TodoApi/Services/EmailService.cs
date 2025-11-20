namespace TodoApi.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string email, string token)
    {
        var verificationUrl = $"http://localhost:5173/verify?token={token}";

        _logger.LogInformation(verificationUrl);

        return Task.CompletedTask;
    }
}
