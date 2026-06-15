using AndyTipster.Application.Auth.DTOs;
using AndyTipster.Application.Auth.Services;
using AndyTipster.Application.Auth.Validators;
using AndyTipster.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Implements authentication operations using ASP.NET Core Identity.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        IEmailService emailService,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResult<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Validate password complexity
        var passwordValidation = PasswordValidator.Validate(request.Password);
        if (!passwordValidation.IsValid)
        {
            return AuthResult<RegisterResponse>.ValidationFailure(
                new Dictionary<string, string[]>
                {
                    ["password"] = passwordValidation.Errors.ToArray()
                });
        }

        // Check if email already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return AuthResult<RegisterResponse>.Failure(
                "An account with this email address already exists.",
                409);
        }

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return AuthResult<RegisterResponse>.ValidationFailure(errors);
        }

        // Assign "Free User" role by default
        await AssignDefaultRoleAsync(user);

        // Generate email confirmation token and send verification email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendVerificationEmailAsync(user.Email!, user.Id.ToString(), token, cancellationToken);

        _logger.LogInformation("User {Email} registered successfully. Verification email sent.", user.Email);

        return AuthResult<RegisterResponse>.Success(
            new RegisterResponse(user.Id, user.Email!, "Registration successful. Please check your email to verify your account."),
            201);
    }

    public async Task<AuthResult> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            return AuthResult.Failure("Invalid user ID.", 400);
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return AuthResult.Failure("Invalid verification link.", 400);
        }

        if (user.EmailConfirmed)
        {
            return AuthResult.Failure("Email is already verified.", 400);
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        if (!result.Succeeded)
        {
            // Token is invalid or expired
            var errorMessage = result.Errors.Any(e => e.Code == "InvalidToken")
                ? "Verification link has expired or has already been used. Please request a new verification email."
                : "Email verification failed. Please request a new verification email.";

            return AuthResult.Failure(errorMessage, 400);
        }

        _logger.LogInformation("User {Email} email verified successfully.", user.Email);

        return AuthResult.Success();
    }

    public async Task<AuthResult> ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            // Don't reveal whether the email exists for security
            return AuthResult.Success();
        }

        if (user.EmailConfirmed)
        {
            // Don't reveal that the email is already verified
            return AuthResult.Success();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _emailService.SendVerificationEmailAsync(user.Email!, user.Id.ToString(), token, cancellationToken);

        _logger.LogInformation("Verification email resent to {Email}.", user.Email);

        return AuthResult.Success();
    }

    public async Task<bool> IsEmailVerifiedAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user?.EmailConfirmed ?? false;
    }

    public async Task<AuthResult<LoginResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult<LoginResponse>.Failure("Invalid email or password.", 401);
        }

        // Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Login attempt for locked account {Email}.", user.Email);
            return AuthResult<LoginResponse>.Failure(
                "Your account has been temporarily locked due to multiple failed login attempts. Please try again in 15 minutes.",
                401);
        }

        // Validate password
        var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValidPassword)
        {
            // Record failed attempt
            await _userManager.AccessFailedAsync(user);

            // Check if this attempt triggered a lockout
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {Email} locked out after failed login attempts.", user.Email);
                await _emailService.SendAccountLockoutNotificationAsync(user.Email!, cancellationToken);
                return AuthResult<LoginResponse>.Failure(
                    "Your account has been temporarily locked due to multiple failed login attempts. Please try again in 15 minutes.",
                    401);
            }

            return AuthResult<LoginResponse>.Failure("Invalid email or password.", 401);
        }

        // Check email verification
        if (!user.EmailConfirmed)
        {
            return AuthResult<LoginResponse>.Failure(
                "Please verify your email address before logging in. Check your inbox for the verification email or request a new one.",
                401);
        }

        // Check if account is suspended
        if (user.IsSuspended)
        {
            return AuthResult<LoginResponse>.Failure("Your account has been suspended. Please contact support.", 401);
        }

        // Reset failed access count on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

        _logger.LogInformation("User {Email} logged in successfully.", user.Email);

        return AuthResult<LoginResponse>.Success(
            new LoginResponse(accessToken, refreshToken, expiresAt));
    }

    public async Task<AuthResult<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var (user, oldToken) = await _tokenService.ValidateRefreshTokenAsync(request.RefreshToken);

        if (user is null || oldToken is null)
        {
            return AuthResult<LoginResponse>.Failure("Invalid or expired refresh token.", 401);
        }

        // Check if user account is still active
        if (user.IsSuspended || user.IsDeleted)
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress);
            return AuthResult<LoginResponse>.Failure("Account is no longer active.", 401);
        }

        // Rotate: generate new tokens
        var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
        var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

        // Revoke old refresh token (rotation — old token invalidated, replaced by new)
        await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken, ipAddress, newRefreshToken);

        _logger.LogInformation("Token refreshed for user {Email}.", user.Email);

        return AuthResult<LoginResponse>.Success(
            new LoginResponse(accessToken, newRefreshToken, expiresAt));
    }

    public async Task<AuthResult> LogoutAsync(string refreshToken, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress);

        return AuthResult.Success();
    }

    public async Task<AuthResult> RequestPasswordResetAsync(PasswordResetRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Don't reveal whether the email exists — always return success
            return AuthResult.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(user.Email!, token, cancellationToken);

        _logger.LogInformation("Password reset requested for {Email}.", user.Email);

        return AuthResult.Success();
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return AuthResult.Failure("Invalid password reset request.", 400);
        }

        // Validate new password complexity
        var passwordValidation = PasswordValidator.Validate(request.NewPassword);
        if (!passwordValidation.IsValid)
        {
            return AuthResult.Failure(passwordValidation.Errors.First(), 400);
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.Any(e => e.Code == "InvalidToken")
                ? "Password reset link has expired or has already been used. Please request a new one."
                : result.Errors.First().Description;

            return AuthResult.Failure(errorMessage, 400);
        }

        _logger.LogInformation("Password reset successful for {Email}.", user.Email);

        return AuthResult.Success();
    }

    private async Task AssignDefaultRoleAsync(ApplicationUser user)
    {
        const string defaultRoleName = "Free User";

        // Ensure the role exists
        if (!await _roleManager.RoleExistsAsync(defaultRoleName))
        {
            var role = new Role
            {
                Name = defaultRoleName,
                NormalizedName = defaultRoleName.ToUpperInvariant(),
                Description = "Default role for newly registered users",
                HierarchyLevel = 5,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow
            };
            await _roleManager.CreateAsync(role);
        }

        await _userManager.AddToRoleAsync(user, defaultRoleName);
    }
}
