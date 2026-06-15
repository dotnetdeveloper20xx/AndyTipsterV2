using AndyTipster.Application.Auth.DTOs;
using AndyTipster.Application.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("AuthRateLimit")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user account. Sends a verification email upon success.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Display name is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ValidationErrors is not null)
            {
                return ValidationProblem(
                    type: "https://andytipster.com/errors/validation-failed",
                    title: "Validation Failed",
                    detail: "One or more validation errors occurred.",
                    modelStateDictionary: ToModelState(result.ValidationErrors),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return Problem(
                type: result.StatusCode == 409
                    ? "https://andytipster.com/errors/email-already-exists"
                    : "https://andytipster.com/errors/registration-failed",
                title: result.StatusCode == 409 ? "Conflict" : "Registration Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Verify a user's email address using the token from the verification email.
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "User ID and token are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _authService.VerifyEmailAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/verification-failed",
                title: "Email Verification Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(new { message = "Email verified successfully. You can now log in." });
    }

    /// <summary>
    /// Resend verification email to the specified address.
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await _authService.ResendVerificationEmailAsync(request.Email, cancellationToken);

        // Always return success to avoid email enumeration
        return Ok(new { message = "If the email is registered and unverified, a new verification email has been sent." });
    }

    /// <summary>
    /// Authenticate user with email and password. Returns JWT access token and refresh token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email and password are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ipAddress = GetClientIpAddress();
        var result = await _authService.LoginAsync(request, ipAddress, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/authentication-failed",
                title: "Authentication Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Refresh access token using a valid refresh token. Implements token rotation.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Refresh token is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ipAddress = GetClientIpAddress();
        var result = await _authService.RefreshTokenAsync(request, ipAddress, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/token-refresh-failed",
                title: "Token Refresh Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Revoke the specified refresh token (logout).
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Refresh token is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ipAddress = GetClientIpAddress();
        await _authService.LogoutAsync(request.RefreshToken, ipAddress, cancellationToken);

        return Ok(new { message = "Logged out successfully." });
    }

    /// <summary>
    /// Authenticate via social provider (Google, Facebook, Apple). Creates or links account based on email match.
    /// </summary>
    [HttpPost("social-login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Provider and access token are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ipAddress = GetClientIpAddress();
        var result = await _authService.SocialLoginAsync(request, ipAddress, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/social-login-failed",
                title: "Social Login Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Request a password reset email. Always returns success to prevent email enumeration.
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await _authService.RequestPasswordResetAsync(request, cancellationToken);

        // Always return success to prevent email enumeration
        return Ok(new { message = "If the email is registered, a password reset link has been sent." });
    }

    /// <summary>
    /// Reset password using the token from the reset email.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email, token, and new password are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await _authService.ResetPasswordAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/password-reset-failed",
                title: "Password Reset Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(new { message = "Password has been reset successfully. You can now log in with your new password." });
    }

    /// <summary>
    /// Initiate 2FA setup. Returns QR code URI and manual entry key.
    /// </summary>
    [Authorize]
    [HttpPost("2fa/enable")]
    [ProducesResponseType(typeof(Enable2FAResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enable2FA(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Problem(
                type: "https://andytipster.com/errors/authentication-failed",
                title: "Authentication Failed",
                detail: "User identity not found.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _authService.Enable2FAAsync(userId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/2fa-setup-failed",
                title: "2FA Setup Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Confirm 2FA setup by verifying a TOTP code. Activates 2FA and returns recovery codes.
    /// </summary>
    [Authorize]
    [HttpPost("2fa/confirm")]
    [ProducesResponseType(typeof(RecoveryCodesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirm2FA([FromBody] Verify2FARequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Verification code is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Problem(
                type: "https://andytipster.com/errors/authentication-failed",
                title: "Authentication Failed",
                detail: "User identity not found.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _authService.Confirm2FASetupAsync(userId, request.Code, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/2fa-confirmation-failed",
                title: "2FA Confirmation Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Verify TOTP code during login flow. Issues full token pair on success.
    /// </summary>
    [HttpPost("2fa/verify")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Verify2FALogin([FromBody] Verify2FALoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email and verification code are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ipAddress = GetClientIpAddress();
        var result = await _authService.Verify2FALoginAsync(request.Email, request.Code, ipAddress, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/2fa-verification-failed",
                title: "2FA Verification Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Verify recovery code during login flow. Issues full token pair on success.
    /// </summary>
    [HttpPost("2fa/recovery")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> VerifyRecoveryCode([FromBody] VerifyRecoveryCodeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.RecoveryCode))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Email and recovery code are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ipAddress = GetClientIpAddress();
        var result = await _authService.VerifyRecoveryCodeAsync(request.Email, request.RecoveryCode, ipAddress, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/recovery-code-failed",
                title: "Recovery Code Verification Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Disable 2FA after password confirmation.
    /// </summary>
    [Authorize]
    [HttpPost("2fa/disable")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Disable2FA([FromBody] Disable2FARequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Password is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Problem(
                type: "https://andytipster.com/errors/authentication-failed",
                title: "Authentication Failed",
                detail: "User identity not found.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await _authService.Disable2FAAsync(userId, request.Password, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/2fa-disable-failed",
                title: "2FA Disable Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(new { message = "Two-factor authentication has been disabled." });
    }

    private string? GetClientIpAddress()
    {
        // Check for forwarded header first (behind proxy/load balancer)
        var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').First().Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary ToModelState(Dictionary<string, string[]> errors)
    {
        var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        foreach (var (key, messages) in errors)
        {
            foreach (var message in messages)
            {
                modelState.AddModelError(key, message);
            }
        }
        return modelState;
    }
}
