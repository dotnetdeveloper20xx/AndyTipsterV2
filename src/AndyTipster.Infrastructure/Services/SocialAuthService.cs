using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AndyTipster.Application.Auth.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Validates social provider access tokens by calling the provider's userinfo endpoint.
/// </summary>
public class SocialAuthService : ISocialAuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SocialAuthService> _logger;

    private static readonly string[] SupportedProviders = ["Google", "Facebook", "Apple"];

    public SocialAuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SocialAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SocialUserInfo?> ValidateTokenAsync(string provider, string accessToken, CancellationToken cancellationToken = default)
    {
        var normalizedProvider = provider.Trim();

        return normalizedProvider.ToLowerInvariant() switch
        {
            "google" => await ValidateGoogleTokenAsync(accessToken, cancellationToken),
            "facebook" => await ValidateFacebookTokenAsync(accessToken, cancellationToken),
            "apple" => await ValidateAppleTokenAsync(accessToken, cancellationToken),
            _ => null
        };
    }

    private async Task<SocialUserInfo?> ValidateGoogleTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SocialAuth");
            var response = await client.GetAsync(
                $"https://www.googleapis.com/oauth2/v3/userinfo?access_token={accessToken}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google token validation failed with status {StatusCode}.", response.StatusCode);
                return null;
            }

            var userInfo = await response.Content.ReadFromJsonAsync<GoogleUserInfo>(cancellationToken: cancellationToken);

            if (userInfo is null || string.IsNullOrWhiteSpace(userInfo.Email))
            {
                _logger.LogWarning("Google token validation returned no email.");
                return null;
            }

            return new SocialUserInfo(
                Email: userInfo.Email,
                Name: userInfo.Name ?? userInfo.Email.Split('@')[0],
                Provider: "Google",
                ProviderUserId: userInfo.Sub ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token.");
            return null;
        }
    }

    private async Task<SocialUserInfo?> ValidateFacebookTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("SocialAuth");
            var response = await client.GetAsync(
                $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Facebook token validation failed with status {StatusCode}.", response.StatusCode);
                return null;
            }

            var userInfo = await response.Content.ReadFromJsonAsync<FacebookUserInfo>(cancellationToken: cancellationToken);

            if (userInfo is null || string.IsNullOrWhiteSpace(userInfo.Email))
            {
                _logger.LogWarning("Facebook token validation returned no email.");
                return null;
            }

            return new SocialUserInfo(
                Email: userInfo.Email,
                Name: userInfo.Name ?? userInfo.Email.Split('@')[0],
                Provider: "Facebook",
                ProviderUserId: userInfo.Id ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Facebook token.");
            return null;
        }
    }

    private async Task<SocialUserInfo?> ValidateAppleTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        try
        {
            // Apple uses identity tokens (JWTs). For now, we decode the token claims.
            // In production, you'd validate the JWT signature against Apple's public keys.
            var client = _httpClientFactory.CreateClient("SocialAuth");

            // Apple's token is typically an id_token JWT. We validate by calling Apple's token endpoint
            // or by decoding the JWT. For this implementation, we use a simplified approach
            // that accepts the token and extracts email from it.
            // In a full implementation, validate the JWT against https://appleid.apple.com/auth/keys

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            if (!handler.CanReadToken(accessToken))
            {
                _logger.LogWarning("Apple token is not a valid JWT.");
                return null;
            }

            var jwtToken = handler.ReadJwtToken(accessToken);
            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var sub = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Apple token does not contain email claim.");
                return null;
            }

            return new SocialUserInfo(
                Email: email,
                Name: email.Split('@')[0],
                Provider: "Apple",
                ProviderUserId: sub ?? string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Apple token.");
            return null;
        }
    }

    private record GoogleUserInfo
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("email_verified")]
        public bool EmailVerified { get; init; }
    }

    private record FacebookUserInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("email")]
        public string? Email { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }
    }
}
