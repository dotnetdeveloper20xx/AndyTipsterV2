using AndyTipster.Application.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Stub email service that logs email content. 
/// Will be replaced with SendGrid implementation later.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendVerificationEmailAsync(string email, string userId, string token, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["App:FrontendUrl"] ?? "https://localhost:4200";
        var encodedToken = Uri.EscapeDataString(token);
        var verificationUrl = $"{baseUrl}/auth/verify-email?userId={userId}&token={encodedToken}";

        _logger.LogInformation(
            "[EMAIL STUB] Verification email for {Email}. Verification URL: {VerificationUrl}",
            email,
            verificationUrl);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["App:FrontendUrl"] ?? "https://localhost:4200";
        var encodedToken = Uri.EscapeDataString(token);
        var resetUrl = $"{baseUrl}/auth/reset-password?email={Uri.EscapeDataString(email)}&token={encodedToken}";

        _logger.LogInformation(
            "[EMAIL STUB] Password reset email for {Email}. Reset URL: {ResetUrl}",
            email,
            resetUrl);

        return Task.CompletedTask;
    }

    public Task SendAccountLockoutNotificationAsync(string email, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Account lockout notification for {Email}. Your account has been temporarily locked due to multiple failed login attempts.",
            email);

        return Task.CompletedTask;
    }
}
