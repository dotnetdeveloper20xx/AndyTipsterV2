namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Request to verify a TOTP code during login when 2FA is enabled.
/// </summary>
public record Verify2FALoginRequest(string Email, string Code);
