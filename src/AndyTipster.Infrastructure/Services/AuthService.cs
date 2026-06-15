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
    private readonly ISocialAuthService _socialAuthService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<Role> roleManager,
        IEmailService emailService,
        ITokenService tokenService,
        ISocialAuthService socialAuthService,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _tokenService = tokenService;
        _socialAuthService = socialAuthService;
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

        // Check if 2FA is enabled — if so, don't issue tokens yet
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            _logger.LogInformation("User {Email} requires 2FA verification.", user.Email);
            return AuthResult<LoginResponse>.Success(
                new LoginResponse(
                    AccessToken: string.Empty,
                    RefreshToken: string.Empty,
                    ExpiresAt: DateTime.MinValue,
                    RequiresTwoFactor: true,
                    TwoFactorEmail: user.Email));
        }

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

    public async Task<AuthResult<LoginResponse>> SocialLoginAsync(SocialLoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.AccessToken))
        {
            return AuthResult<LoginResponse>.Failure("Provider and access token are required.", 400);
        }

        // Validate the social token with the provider
        var socialUser = await _socialAuthService.ValidateTokenAsync(request.Provider, request.AccessToken, cancellationToken);

        if (socialUser is null)
        {
            return AuthResult<LoginResponse>.Failure("Invalid social login token.", 401);
        }

        // Check if user with that email already exists
        var existingUser = await _userManager.FindByEmailAsync(socialUser.Email);

        if (existingUser is not null)
        {
            // Link the social provider to the existing account (if not already linked)
            var existingLogins = await _userManager.GetLoginsAsync(existingUser);
            var alreadyLinked = existingLogins.Any(l =>
                l.LoginProvider == socialUser.Provider &&
                l.ProviderKey == socialUser.ProviderUserId);

            if (!alreadyLinked)
            {
                var loginInfo = new UserLoginInfo(socialUser.Provider, socialUser.ProviderUserId, socialUser.Provider);
                var addLoginResult = await _userManager.AddLoginAsync(existingUser, loginInfo);

                if (!addLoginResult.Succeeded)
                {
                    _logger.LogWarning("Failed to link {Provider} to existing account {Email}.", socialUser.Provider, socialUser.Email);
                }
                else
                {
                    _logger.LogInformation("Linked {Provider} to existing account {Email}.", socialUser.Provider, socialUser.Email);
                }
            }

            // Check if account is suspended
            if (existingUser.IsSuspended)
            {
                return AuthResult<LoginResponse>.Failure("Your account has been suspended. Please contact support.", 401);
            }

            // Update last login
            existingUser.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(existingUser);

            // Generate tokens
            var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(existingUser);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(existingUser, ipAddress);

            _logger.LogInformation("User {Email} logged in via {Provider}.", existingUser.Email, socialUser.Provider);

            return AuthResult<LoginResponse>.Success(
                new LoginResponse(accessToken, refreshToken, expiresAt));
        }
        else
        {
            // Create new account — email is already verified via social provider
            var newUser = new ApplicationUser
            {
                UserName = socialUser.Email,
                Email = socialUser.Email,
                DisplayName = socialUser.Name,
                EmailConfirmed = true, // Social providers verify email
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create account for social login {Email}: {Errors}", socialUser.Email, errors);
                return AuthResult<LoginResponse>.Failure("Failed to create account. Please try again.", 500);
            }

            // Link the social provider
            var loginInfo = new UserLoginInfo(socialUser.Provider, socialUser.ProviderUserId, socialUser.Provider);
            await _userManager.AddLoginAsync(newUser, loginInfo);

            // Assign "Free User" role by default
            await AssignDefaultRoleAsync(newUser);

            // Update last login
            newUser.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(newUser);

            // Generate tokens
            var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(newUser);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(newUser, ipAddress);

            _logger.LogInformation("New user {Email} created via {Provider} social login.", socialUser.Email, socialUser.Provider);

            return AuthResult<LoginResponse>.Success(
                new LoginResponse(accessToken, refreshToken, expiresAt));
        }
    }

    public async Task<AuthResult<Enable2FAResponse>> Enable2FAAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthResult<Enable2FAResponse>.Failure("User not found.", 404);
        }

        // Check if 2FA is already enabled
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return AuthResult<Enable2FAResponse>.Failure("Two-factor authentication is already enabled.", 400);
        }

        // Reset the authenticator key to generate a new one
        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(key))
        {
            return AuthResult<Enable2FAResponse>.Failure("Failed to generate authenticator key.", 500);
        }

        // Generate QR code URI in otpauth format
        var email = user.Email ?? user.UserName ?? "user";
        var qrCodeUri = $"otpauth://totp/AndyTipster:{email}?secret={key}&issuer=AndyTipster";

        _logger.LogInformation("2FA setup initiated for user {Email}.", user.Email);

        return AuthResult<Enable2FAResponse>.Success(
            new Enable2FAResponse(qrCodeUri, key));
    }

    public async Task<AuthResult<RecoveryCodesResponse>> Confirm2FASetupAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthResult<RecoveryCodesResponse>.Failure("User not found.", 404);
        }

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return AuthResult<RecoveryCodesResponse>.Failure("Two-factor authentication is already enabled.", 400);
        }

        // Verify the TOTP code provided by the user
        var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!isValidCode)
        {
            return AuthResult<RecoveryCodesResponse>.Failure("Invalid verification code. Please try again.", 400);
        }

        // Activate 2FA
        await _userManager.SetTwoFactorEnabledAsync(user, true);

        // Generate 8 single-use recovery codes
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 8);

        _logger.LogInformation("2FA activated for user {Email}. Recovery codes generated.", user.Email);

        return AuthResult<RecoveryCodesResponse>.Success(
            new RecoveryCodesResponse(recoveryCodes?.ToList() ?? new List<string>()));
    }

    public async Task<AuthResult<LoginResponse>> Verify2FALoginAsync(string email, string code, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return AuthResult<LoginResponse>.Failure("Invalid verification attempt.", 401);
        }

        // Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            return AuthResult<LoginResponse>.Failure(
                "Your account has been temporarily locked due to multiple failed attempts. Please try again in 15 minutes.",
                401);
        }

        // Verify the TOTP code with ±1 clock skew tolerance (handled by Identity's authenticator token provider)
        var isValidCode = await _userManager.VerifyTwoFactorTokenAsync(
            user,
            _userManager.Options.Tokens.AuthenticatorTokenProvider,
            code);

        if (!isValidCode)
        {
            // Record failed attempt — triggers lockout after 5 failures
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {Email} locked out after failed 2FA attempts.", user.Email);
                await _emailService.SendAccountLockoutNotificationAsync(user.Email!, cancellationToken);
                return AuthResult<LoginResponse>.Failure(
                    "Your account has been temporarily locked due to multiple failed attempts. Please try again in 15 minutes.",
                    401);
            }

            return AuthResult<LoginResponse>.Failure("Invalid verification code.", 401);
        }

        // Reset failed access count on successful verification
        await _userManager.ResetAccessFailedCountAsync(user);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

        _logger.LogInformation("User {Email} completed 2FA login successfully.", user.Email);

        return AuthResult<LoginResponse>.Success(
            new LoginResponse(accessToken, refreshToken, expiresAt));
    }

    public async Task<AuthResult<LoginResponse>> VerifyRecoveryCodeAsync(string email, string recoveryCode, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return AuthResult<LoginResponse>.Failure("Invalid verification attempt.", 401);
        }

        // Check if account is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            return AuthResult<LoginResponse>.Failure(
                "Your account has been temporarily locked due to multiple failed attempts. Please try again in 15 minutes.",
                401);
        }

        // Redeem the recovery code (marks it as consumed)
        var result = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, recoveryCode);

        if (!result.Succeeded)
        {
            // Record failed attempt
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Account {Email} locked out after failed recovery code attempts.", user.Email);
                await _emailService.SendAccountLockoutNotificationAsync(user.Email!, cancellationToken);
                return AuthResult<LoginResponse>.Failure(
                    "Your account has been temporarily locked due to multiple failed attempts. Please try again in 15 minutes.",
                    401);
            }

            return AuthResult<LoginResponse>.Failure("Invalid recovery code.", 401);
        }

        // Reset failed access count
        await _userManager.ResetAccessFailedCountAsync(user);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var (accessToken, expiresAt) = await _tokenService.GenerateAccessTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

        // Check remaining recovery codes — warn if fewer than 2 remain
        var remainingCodes = await _userManager.CountRecoveryCodesAsync(user);
        var warningMessage = remainingCodes < 2
            ? "Warning: You have fewer than 2 recovery codes remaining. Please re-register your authenticator app to generate new codes."
            : null;

        _logger.LogInformation("User {Email} logged in via recovery code. {Remaining} codes remaining.", user.Email, remainingCodes);

        // Return tokens — if warning needed, we include it in a custom way via the response
        // The frontend can check recovery code count separately, but we log it
        if (remainingCodes < 2)
        {
            _logger.LogWarning("User {Email} has fewer than 2 recovery codes remaining. Re-registration recommended.", user.Email);
        }

        return AuthResult<LoginResponse>.Success(
            new LoginResponse(accessToken, refreshToken, expiresAt));
    }

    public async Task<AuthResult> Disable2FAAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return AuthResult.Failure("User not found.", 404);
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            return AuthResult.Failure("Two-factor authentication is not enabled.", 400);
        }

        // Confirm password before allowing 2FA disable
        var isValidPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!isValidPassword)
        {
            return AuthResult.Failure("Invalid password.", 401);
        }

        // Disable 2FA and reset the authenticator key
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        _logger.LogInformation("2FA disabled for user {Email}.", user.Email);

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
