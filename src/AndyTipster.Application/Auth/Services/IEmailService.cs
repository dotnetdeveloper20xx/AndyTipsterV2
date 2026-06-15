namespace AndyTipster.Application.Auth.Services;

/// <summary>
/// Abstraction for sending emails. Implementations can use SendGrid, SMTP, or a stub logger.
/// </summary>
public interface IEmailService
{
    Task SendVerificationEmailAsync(string email, string userId, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default);
    Task SendAccountLockoutNotificationAsync(string email, CancellationToken cancellationToken = default);
}
