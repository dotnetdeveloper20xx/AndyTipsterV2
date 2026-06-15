namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Request to verify a TOTP code during 2FA setup confirmation.
/// </summary>
public record Verify2FARequest(string Code);
