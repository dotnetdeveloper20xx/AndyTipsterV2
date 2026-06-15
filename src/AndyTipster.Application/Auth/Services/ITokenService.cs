using AndyTipster.Domain.Entities;

namespace AndyTipster.Application.Auth.Services;

/// <summary>
/// Service interface for JWT token generation and refresh token management.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token with user claims (sub, email, roles, permissions). 15-minute expiry.
    /// </summary>
    Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user);

    /// <summary>
    /// Generates a cryptographically random refresh token and stores it in the database. 7-day expiry.
    /// </summary>
    Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string? ipAddress = null);

    /// <summary>
    /// Validates a refresh token: checks it exists, is not expired/revoked, and returns the associated user.
    /// </summary>
    Task<(ApplicationUser? User, RefreshToken? Token)> ValidateRefreshTokenAsync(string token);

    /// <summary>
    /// Revokes a specific refresh token.
    /// </summary>
    Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null);
}
