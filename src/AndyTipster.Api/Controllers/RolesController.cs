using System.Security.Claims;
using AndyTipster.Application.Roles.DTOs;
using AndyTipster.Application.Roles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
[EnableRateLimiting("GeneralRateLimit")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// List all roles with permissions and user counts. Requires Admin+ role.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Super Admin,Admin")]
    [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetAllRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Get role details with permissions by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Super Admin,Admin")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRoleByIdAsync(id, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/role-not-found",
                title: "Role Not Found",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a custom role with selected permissions. Super Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Super Admin")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Role name is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var actorUserId = GetCurrentUserId();
        var result = await _roleService.CreateRoleAsync(request, actorUserId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: result.StatusCode == 409
                    ? "https://andytipster.com/errors/role-already-exists"
                    : "https://andytipster.com/errors/role-creation-failed",
                title: result.StatusCode == 409 ? "Conflict" : "Role Creation Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Update an existing role. Super Admin only.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Super Admin")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            return Problem(
                type: "https://andytipster.com/errors/validation-failed",
                title: "Validation Failed",
                detail: "Role name is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var actorUserId = GetCurrentUserId();
        var result = await _roleService.UpdateRoleAsync(id, request, actorUserId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/role-update-failed",
                title: "Role Update Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Delete a role. Super Admin only. Role must not be in use or a system role.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Super Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var result = await _roleService.DeleteRoleAsync(id, actorUserId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/role-deletion-failed",
                title: "Role Deletion Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(new { message = "Role deleted successfully." });
    }

    /// <summary>
    /// Assign a role to a user. Admin+ required. Enforces role hierarchy.
    /// </summary>
    [HttpPost("assign")]
    [Authorize(Roles = "Super Admin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var result = await _roleService.AssignRoleToUserAsync(request, actorUserId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/role-assignment-failed",
                title: "Role Assignment Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(new { message = "Role assigned successfully." });
    }

    /// <summary>
    /// Remove a role from a user. Admin+ required. Enforces role hierarchy.
    /// </summary>
    [HttpPost("remove")]
    [Authorize(Roles = "Super Admin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleRequest request, CancellationToken cancellationToken)
    {
        var actorUserId = GetCurrentUserId();
        var result = await _roleService.RemoveRoleFromUserAsync(request, actorUserId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/role-removal-failed",
                title: "Role Removal Failed",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(new { message = "Role removed successfully." });
    }

    /// <summary>
    /// Get effective permissions for a user (union of all assigned roles' permissions).
    /// </summary>
    [HttpGet("user/{userId:guid}/permissions")]
    [Authorize(Roles = "Super Admin,Admin")]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _roleService.GetUserPermissionsAsync(userId, cancellationToken);

        if (!result.Succeeded)
        {
            return Problem(
                type: "https://andytipster.com/errors/user-not-found",
                title: "User Not Found",
                detail: result.ErrorMessage,
                statusCode: result.StatusCode);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all available permissions.
    /// </summary>
    [HttpGet("permissions")]
    [Authorize(Roles = "Super Admin,Admin")]
    [ProducesResponseType(typeof(List<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken cancellationToken)
    {
        var permissions = await _roleService.GetAllPermissionsAsync(cancellationToken);
        return Ok(permissions);
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
