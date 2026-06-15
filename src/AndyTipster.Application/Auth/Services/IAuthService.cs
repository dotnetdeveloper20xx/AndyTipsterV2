using AndyTipster.Application.Auth.DTOs;

namespace AndyTipster.Application.Auth.Services;

/// <summary>
/// Service interface for authentication operations including registration, email verification,
/// login, token refresh, password reset, and two-factor authentication.
/// </summary>
public interface IAuthService
{
    Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> IsEmailVerifiedAsync(string email, CancellationToken cancellationToken = default);
    Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResult> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResult> RequestPasswordResetAsync(PasswordResetRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult<LoginResponse>> SocialLoginAsync(SocialLoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);

    // Two-Factor Authentication
    Task<AuthResult<Enable2FAResponse>> Enable2FAAsync(string userId, CancellationToken cancellationToken = default);
    Task<AuthResult<RecoveryCodesResponse>> Confirm2FASetupAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<AuthResult<LoginResponse>> Verify2FALoginAsync(string email, string code, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResult<LoginResponse>> VerifyRecoveryCodeAsync(string email, string recoveryCode, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthResult> Disable2FAAsync(string userId, string password, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result type for auth operations that return data.
/// </summary>
public record AuthResult<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
    public int StatusCode { get; init; } = 200;

    public static AuthResult<T> Success(T data, int statusCode = 200) => new()
    {
        Succeeded = true,
        Data = data,
        StatusCode = statusCode
    };

    public static AuthResult<T> Failure(string error, int statusCode = 400) => new()
    {
        Succeeded = false,
        ErrorMessage = error,
        StatusCode = statusCode
    };

    public static AuthResult<T> ValidationFailure(Dictionary<string, string[]> errors) => new()
    {
        Succeeded = false,
        ValidationErrors = errors,
        StatusCode = 400
    };
}

/// <summary>
/// Result type for auth operations that don't return data.
/// </summary>
public record AuthResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; } = 200;

    public static AuthResult Success(int statusCode = 200) => new()
    {
        Succeeded = true,
        StatusCode = statusCode
    };

    public static AuthResult Failure(string error, int statusCode = 400) => new()
    {
        Succeeded = false,
        ErrorMessage = error,
        StatusCode = statusCode
    };
}
