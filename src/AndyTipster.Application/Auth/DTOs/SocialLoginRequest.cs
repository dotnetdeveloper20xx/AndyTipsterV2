namespace AndyTipster.Application.Auth.DTOs;

/// <summary>
/// Request for social login (Google, Facebook, Apple).
/// The access token is obtained from the OAuth flow on the frontend.
/// </summary>
public record SocialLoginRequest(
    string Provider,
    string AccessToken
);
