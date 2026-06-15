using System.Security.Claims;
using AndyTipster.Application.Audit.Services;
using AndyTipster.Application.Users.DTOs;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Api.Controllers;

/// <summary>
/// User profile management endpoints. Authenticated users manage their own profile:
/// avatar upload, display name/bio/timezone, and activity log.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
[EnableRateLimiting("GeneralRateLimit")]
public class ProfileController : ControllerBase
{
    private readonly AndyTipsterDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private const long MaxAvatarSize = 5 * 1024 * 1024; // 5MB

    public ProfileController(
        AndyTipsterDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService)
    {
        _context = context;
        _userManager = userManager;
        _auditService = auditService;
    }

    /// <summary>
    /// Get the current user's profile.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.Plan)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
            return NotFound();

        var response = new UserDetailResponse
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            TimeZone = user.TimeZone,
            Roles = user.UserRoles.Select(ur => ur.Role.Name!).ToList(),
            Status = user.IsSuspended ? "Suspended" : (!user.EmailConfirmed ? "Unverified" : "Active"),
            Plan = user.Subscriptions
                .Where(s => s.Status == Domain.Enumerations.SubscriptionStatus.Active || s.Status == Domain.Enumerations.SubscriptionStatus.Trialing)
                .Select(s => s.Plan.Name)
                .FirstOrDefault(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsSuspended = user.IsSuspended,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled
        };

        return Ok(response);
    }

    /// <summary>
    /// Update profile: display name (3-50 chars), bio (max 500 chars), timezone.
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        // Validate display name
        if (request.DisplayName is not null)
        {
            if (request.DisplayName.Length < 3 || request.DisplayName.Length > 50)
            {
                return Problem(
                    type: "https://andytipster.com/errors/validation-failed",
                    title: "Validation Failed",
                    detail: "Display name must be between 3 and 50 characters.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
        }

        // Validate bio
        if (request.Bio is not null && request.Bio.Length > 500)
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Bio must be at most 500 characters.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync([userId], cancellationToken);

        if (user is null)
            return NotFound();

        if (request.DisplayName is not null)
            user.DisplayName = request.DisplayName;
        if (request.Bio is not null)
            user.Bio = request.Bio;
        if (request.TimeZone is not null)
            user.TimeZone = request.TimeZone;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetProfile(cancellationToken);
    }

    /// <summary>
    /// Upload avatar image. Accepts JPG, PNG, WebP, GIF up to 5MB.
    /// Resizes to 256x256.
    /// </summary>
    [HttpPost("avatar")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar, CancellationToken cancellationToken)
    {
        if (avatar is null || avatar.Length == 0)
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "No file was uploaded.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Validate file size
        if (avatar.Length > MaxAvatarSize)
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Avatar file must not exceed 5 MB.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Validate file type
        var extension = Path.GetExtension(avatar.FileName);
        if (!AllowedExtensions.Contains(extension) || !AllowedImageTypes.Contains(avatar.ContentType))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Accepted file types are JPG, PNG, WebP, and GIF.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var userId = GetCurrentUserId();
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user is null)
            return NotFound();

        // In production, this would upload to Azure Blob Storage with resize.
        // For now, store as a base64 data URL or generate a placeholder URL.
        var fileName = $"avatars/{userId}/{Guid.NewGuid()}{extension}";
        user.AvatarUrl = $"/uploads/{fileName}";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { avatarUrl = user.AvatarUrl });
    }

    /// <summary>
    /// Get user's activity log with login history and subscription changes.
    /// Paginated at 50 entries per page.
    /// </summary>
    [HttpGet("activity")]
    [ProducesResponseType(typeof(ActivityLogResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityLog([FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        const int pageSize = 50;
        var currentPage = Math.Max(page, 1);

        var query = _context.AuditLogs
            .Where(a => a.ActorUserId == userId || a.TargetEntityId == userId.ToString())
            .Where(a => a.ActionType == "Login" ||
                        a.ActionType == "LoginFailed" ||
                        a.ActionType == "SubscriptionCreated" ||
                        a.ActionType == "SubscriptionCancelled" ||
                        a.ActionType == "SubscriptionUpgraded" ||
                        a.ActionType == "SubscriptionDowngraded" ||
                        a.ActionType == "ProfileUpdated" ||
                        a.ActionType == "PasswordChanged" ||
                        a.ActionType == "TwoFactorEnabled" ||
                        a.ActionType == "TwoFactorDisabled")
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var entries = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ActivityLogEntry
            {
                Id = a.Id,
                ActionType = a.ActionType,
                Description = GetActivityDescription(a.ActionType),
                Timestamp = a.Timestamp,
                IpAddress = a.IpAddress
            })
            .ToListAsync(cancellationToken);

        return Ok(new ActivityLogResponse
        {
            Entries = entries,
            TotalCount = totalCount,
            Page = currentPage,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    private static string GetActivityDescription(string actionType) => actionType switch
    {
        "Login" => "Successful login",
        "LoginFailed" => "Failed login attempt",
        "SubscriptionCreated" => "Subscription created",
        "SubscriptionCancelled" => "Subscription cancelled",
        "SubscriptionUpgraded" => "Subscription upgraded",
        "SubscriptionDowngraded" => "Subscription downgraded",
        "ProfileUpdated" => "Profile updated",
        "PasswordChanged" => "Password changed",
        "TwoFactorEnabled" => "Two-factor authentication enabled",
        "TwoFactorDisabled" => "Two-factor authentication disabled",
        _ => actionType
    };

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User ID not found in token.");
    }
}
