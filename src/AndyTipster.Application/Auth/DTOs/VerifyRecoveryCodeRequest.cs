namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Request to verify a recovery code during login when 2FA is enabled.
/// </summary>
public record VerifyRecoveryCodeRequest(string Email, string RecoveryCode);
