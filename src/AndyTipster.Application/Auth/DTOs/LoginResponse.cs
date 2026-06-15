namespace AndyTipster.Application.Auth.DTOs;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
