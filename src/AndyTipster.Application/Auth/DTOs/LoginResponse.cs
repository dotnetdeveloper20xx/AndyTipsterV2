namespace AndyTipster.Application.Auth.DTOs;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    bool RequiresTwoFactor = false,
    string? TwoFactorEmail = null
);
