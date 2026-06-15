using System.Security.Claims;
using AndyTipster.Application.Users.DTOs;
using AndyTipster.Application.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AndyTipster.Api.Controllers;

/// <summary>
/// Admin user management endpoints. Provides paginated user listing with search/filter,
/// user impersonation, bulk actions, suspension, and CSV export.
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin,Super Admin")]
[EnableRateLimiting("GeneralRateLimit")]
public class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    /// <summary>
    /// Get paginated list of users with optional search, filter, and sort.
    /// Target: 200ms response time for up to 100,000 users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] UserListRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetUsersAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed user information by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userManagementService.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return Problem(
                type: "https://andytipster.com/errors/user-not-found",
                title: "User Not Found",
                detail: $"User with ID '{id}' was not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(user);
    }

    /// <summary>
    /// Create a read-only impersonation session for the specified user.
    /// Generates a limited token indicating impersonation.
    /// </summary>
    [HttpPost("{id:guid}/impersonate")]
    [Authorize(Roles = "Super Admin")]
    [ProducesResponseType(typeof(ImpersonateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImpersonateUser(Guid id, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var result = await _userManagementService.ImpersonateUserAsync(id, actorUserId, cancellationToken);

        if (result is null)
        {
            return Problem(
                type: "https://andytipster.com/errors/user-not-found",
                title: "User Not Found",
                detail: $"User with ID '{id}' was not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(result);
    }

    /// <summary>
    /// Perform bulk action (suspend, role change, export) on selected users.
    /// Returns summary with success/failure counts and failure reasons.
    /// </summary>
    [HttpPost("bulk-action")]
    [ProducesResponseType(typeof(BulkActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAction([FromBody] BulkActionRequest request, CancellationToken cancellationToken)
    {
        if (request.UserIds.Count == 0)
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "At least one user ID is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Action))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Action type is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var actorUserId = GetCurrentUserId();
        var result = await _userManagementService.ExecuteBulkActionAsync(request, actorUserId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Suspend a user account and revoke all active sessions/tokens within 5 seconds.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendUser(Guid id, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var success = await _userManagementService.SuspendUserAsync(id, actorUserId, cancellationToken);

        if (!success)
        {
            return Problem(
                type: "https://andytipster.com/errors/user-not-found",
                title: "Suspension Failed",
                detail: "User not found or already suspended.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Ok(new { message = "User suspended successfully. All active sessions have been revoked." });
    }

    /// <summary>
    /// Export filtered user data as CSV. Supports up to 100,000 records within 30 seconds.
    /// </summary>
    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportUsers([FromQuery] UserListRequest request, CancellationToken cancellationToken)
    {
        var csvBytes = await _userManagementService.ExportUsersAsync(request, cancellationToken);
        return File(csvBytes, "text/csv", $"users-export-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdClaim, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("User ID not found in token.");
    }
}
