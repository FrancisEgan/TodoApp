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
        var verificationUrl = $"https://localhost:7275/verify?token={token}";

        _logger.LogInformation("===========================================");
        _logger.LogInformation("EMAIL VERIFICATION FOR: {Email}", email);
        _logger.LogInformation("Copy and paste this URL into your browser:");
        _logger.LogInformation("{VerificationUrl}", verificationUrl);
        _logger.LogInformation("Token: {Token}", token);
        _logger.LogInformation("===========================================");

        return Task.CompletedTask;
    }
}
