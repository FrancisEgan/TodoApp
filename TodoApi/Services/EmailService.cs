namespace TodoApi.Services;

public class EmailService : IEmailService
{
    public EmailService()
    { }

    public Task SendVerificationEmailAsync(string email, string token)
    {
        var verificationUrl = $"http://localhost:5173/verify?token={token}";

        Console.WriteLine("Account created for {0}. Verification URL: {1}", email, verificationUrl);

        return Task.CompletedTask;
    }
}
