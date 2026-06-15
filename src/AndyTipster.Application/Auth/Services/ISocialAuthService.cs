namespace AndyTipster.Application.Auth.Services;

/// <summary>
/// Service interface for validating social provider access tokens and extracting user information.
/// </summary>
public interface ISocialAuthService
{
    /// <summary>
    /// Validates a social provider access token and returns the user's email and name.
    /// </summary>
    /// <param name="provider">The provider name (Google, Facebook, Apple).</param>
    /// <param name="accessToken">The access token from the OAuth flow.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The social user info if valid, or null if validation fails.</returns>
    Task<SocialUserInfo?> ValidateTokenAsync(string provider, string accessToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// User information extracted from a social provider token.
/// </summary>
public record SocialUserInfo(
    string Email,
    string Name,
    string Provider,
    string ProviderUserId
);
