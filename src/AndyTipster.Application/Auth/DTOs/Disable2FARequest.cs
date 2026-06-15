namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Request to disable 2FA. Requires password confirmation.
/// </summary>
public record Disable2FARequest(string Password);
