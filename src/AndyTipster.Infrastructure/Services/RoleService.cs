using AndyTipster.Application.Roles.DTOs;
using AndyTipster.Application.Roles.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Implements role and permission management with hierarchy enforcement and audit logging.
/// </summary>
public class RoleService : IRoleService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AndyTipsterDbContext _dbContext;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        RoleManager<Role> roleManager,
        UserManager<ApplicationUser> userManager,
        AndyTipsterDbContext dbContext,
        ILogger<RoleService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .AsNoTracking()
            .OrderBy(r => r.HierarchyLevel)
            .ToListAsync(cancellationToken);

        return roles.Select(MapToRoleResponse).ToList();
    }

    public async Task<RoleResult<RoleResponse>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return RoleResult<RoleResponse>.Failure("Role not found.", 404);
        }

        return RoleResult<RoleResponse>.Success(MapToRoleResponse(role));
    }

    public async Task<RoleResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        // Only Super Admin can create roles
        if (!await IsUserSuperAdminAsync(actorUserId))
        {
            return RoleResult<RoleResponse>.Failure("Only Super Admin can create roles.", 403);
        }

        // Validate unique name
        var existingRole = await _roleManager.FindByNameAsync(request.RoleName);
        if (existingRole is not null)
        {
            return RoleResult<RoleResponse>.Failure($"A role with the name '{request.RoleName}' already exists.", 409);
        }

        // Create role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.RoleName,
            NormalizedName = request.RoleName.ToUpperInvariant(),
            Description = request.Description,
            HierarchyLevel = request.HierarchyLevel,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            return RoleResult<RoleResponse>.Failure($"Failed to create role: {errors}");
        }

        // Assign permissions
        if (request.PermissionIds.Count > 0)
        {
            var permissions = await _dbContext.Permissions
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var permission in permissions)
            {
                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Log audit trail
        await LogAuditAsync(actorUserId, "Role.Created", "Role", role.Id.ToString(),
            beforeJson: null,
            afterJson: System.Text.Json.JsonSerializer.Serialize(new { role.Name, role.HierarchyLevel, role.Description }),
            cancellationToken);

        _logger.LogInformation("Role '{RoleName}' created by user {ActorUserId}.", request.RoleName, actorUserId);

        // Reload with permissions
        var createdRole = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .AsNoTracking()
            .FirstAsync(r => r.Id == role.Id, cancellationToken);

        return RoleResult<RoleResponse>.Success(MapToRoleResponse(createdRole), 201);
    }

    public async Task<RoleResult<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        // Only Super Admin can update roles
        if (!await IsUserSuperAdminAsync(actorUserId))
        {
            return RoleResult<RoleResponse>.Failure("Only Super Admin can update roles.", 403);
        }

        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return RoleResult<RoleResponse>.Failure("Role not found.", 404);
        }

        // Capture before state for audit
        var beforeJson = System.Text.Json.JsonSerializer.Serialize(new { role.Name, role.HierarchyLevel, role.Description });

        // Check for duplicate name (exclude current role)
        var existingRole = await _roleManager.FindByNameAsync(request.RoleName);
        if (existingRole is not null && existingRole.Id != roleId)
        {
            return RoleResult<RoleResponse>.Failure($"A role with the name '{request.RoleName}' already exists.", 409);
        }

        // Update role fields
        role.Name = request.RoleName;
        role.NormalizedName = request.RoleName.ToUpperInvariant();
        role.Description = request.Description;
        role.HierarchyLevel = request.HierarchyLevel;

        // Update permissions: remove existing and add new
        _dbContext.RolePermissions.RemoveRange(role.RolePermissions);

        if (request.PermissionIds.Count > 0)
        {
            var permissions = await _dbContext.Permissions
                .Where(p => request.PermissionIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var permission in permissions)
            {
                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id,
                    AssignedAt = DateTime.UtcNow
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Log audit trail
        var afterJson = System.Text.Json.JsonSerializer.Serialize(new { role.Name, role.HierarchyLevel, role.Description });
        await LogAuditAsync(actorUserId, "Role.Updated", "Role", role.Id.ToString(), beforeJson, afterJson, cancellationToken);

        _logger.LogInformation("Role '{RoleName}' updated by user {ActorUserId}.", request.RoleName, actorUserId);

        // Reload with permissions
        var updatedRole = await _dbContext.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .AsNoTracking()
            .FirstAsync(r => r.Id == role.Id, cancellationToken);

        return RoleResult<RoleResponse>.Success(MapToRoleResponse(updatedRole));
    }

    public async Task<RoleResult> DeleteRoleAsync(Guid roleId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        // Only Super Admin can delete roles
        if (!await IsUserSuperAdminAsync(actorUserId))
        {
            return RoleResult.Failure("Only Super Admin can delete roles.", 403);
        }

        var role = await _dbContext.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            return RoleResult.Failure("Role not found.", 404);
        }

        // Prevent deletion of system roles
        if (role.IsSystem)
        {
            return RoleResult.Failure("System roles cannot be deleted.", 400);
        }

        // Prevent deletion of roles assigned to users
        var userCount = role.UserRoles.Count;
        if (userCount > 0)
        {
            return RoleResult.Failure($"Cannot delete role '{role.Name}' because it is assigned to {userCount} user(s). Remove the role from all users first.", 400);
        }

        // Remove role permissions
        var rolePermissions = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(cancellationToken);
        _dbContext.RolePermissions.RemoveRange(rolePermissions);

        // Delete role
        var deleteResult = await _roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
        {
            var errors = string.Join(", ", deleteResult.Errors.Select(e => e.Description));
            return RoleResult.Failure($"Failed to delete role: {errors}");
        }

        // Log audit trail
        await LogAuditAsync(actorUserId, "Role.Deleted", "Role", roleId.ToString(),
            beforeJson: System.Text.Json.JsonSerializer.Serialize(new { role.Name, role.HierarchyLevel }),
            afterJson: null,
            cancellationToken);

        _logger.LogInformation("Role '{RoleName}' deleted by user {ActorUserId}.", role.Name, actorUserId);

        return RoleResult.Success();
    }

    public async Task<RoleResult> AssignRoleToUserAsync(AssignRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return RoleResult.Failure("User not found.", 404);
        }

        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
        {
            return RoleResult.Failure("Role not found.", 404);
        }

        // Enforce role hierarchy: actor can only assign roles with hierarchy level strictly GREATER than their highest level
        var hierarchyCheck = await EnforceHierarchyAsync(actorUserId, role.HierarchyLevel, cancellationToken);
        if (!hierarchyCheck.Succeeded)
        {
            return hierarchyCheck;
        }

        // Check if user already has this role
        var existingAssignment = await _dbContext.UserRoles
            .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

        if (existingAssignment)
        {
            return RoleResult.Failure("User already has this role assigned.", 400);
        }

        // Assign role
        var result = await _userManager.AddToRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return RoleResult.Failure($"Failed to assign role: {errors}");
        }

        // Update the UserRole record with assignment metadata
        var userRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == request.RoleId, cancellationToken);

        if (userRole is not null)
        {
            userRole.AssignedAt = DateTime.UtcNow;
            userRole.AssignedByUserId = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // Log audit trail
        await LogAuditAsync(actorUserId, "Role.Assigned", "UserRole", request.UserId.ToString(),
            beforeJson: null,
            afterJson: System.Text.Json.JsonSerializer.Serialize(new { UserId = request.UserId, RoleId = request.RoleId, RoleName = role.Name }),
            cancellationToken);

        _logger.LogInformation("Role '{RoleName}' assigned to user {UserId} by {ActorUserId}.", role.Name, request.UserId, actorUserId);

        return RoleResult.Success();
    }

    public async Task<RoleResult> RemoveRoleFromUserAsync(RemoveRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return RoleResult.Failure("User not found.", 404);
        }

        var role = await _dbContext.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken);

        if (role is null)
        {
            return RoleResult.Failure("Role not found.", 404);
        }

        // Enforce role hierarchy
        var hierarchyCheck = await EnforceHierarchyAsync(actorUserId, role.HierarchyLevel, cancellationToken);
        if (!hierarchyCheck.Succeeded)
        {
            return hierarchyCheck;
        }

        // Check if user has this role
        var hasRole = await _userManager.IsInRoleAsync(user, role.Name!);
        if (!hasRole)
        {
            return RoleResult.Failure("User does not have this role.", 400);
        }

        // Remove role
        var result = await _userManager.RemoveFromRoleAsync(user, role.Name!);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return RoleResult.Failure($"Failed to remove role: {errors}");
        }

        // Log audit trail
        await LogAuditAsync(actorUserId, "Role.Removed", "UserRole", request.UserId.ToString(),
            beforeJson: System.Text.Json.JsonSerializer.Serialize(new { UserId = request.UserId, RoleId = request.RoleId, RoleName = role.Name }),
            afterJson: null,
            cancellationToken);

        _logger.LogInformation("Role '{RoleName}' removed from user {UserId} by {ActorUserId}.", role.Name, request.UserId, actorUserId);

        return RoleResult.Success();
    }

    public async Task<RoleResult<List<PermissionResponse>>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return RoleResult<List<PermissionResponse>>.Failure("User not found.", 404);
        }

        // Get union of all permissions across all user's roles
        var permissions = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_dbContext.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp.PermissionId)
            .Distinct()
            .Join(_dbContext.Permissions,
                permId => permId,
                p => p.Id,
                (permId, p) => new PermissionResponse(p.Id, p.Name, p.Description, p.Module))
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return RoleResult<List<PermissionResponse>>.Success(permissions);
    }

    public async Task<List<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionResponse(p.Id, p.Name, p.Description, p.Module))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Enforces role hierarchy: actor can only manage roles with hierarchy level strictly GREATER than their own highest level.
    /// Lower hierarchy level number = higher privilege.
    /// </summary>
    private async Task<RoleResult> EnforceHierarchyAsync(Guid actorUserId, int targetRoleHierarchyLevel, CancellationToken cancellationToken)
    {
        // Get the actor's highest role level (lowest number = highest privilege)
        var actorHighestLevel = await _dbContext.UserRoles
            .Where(ur => ur.UserId == actorUserId)
            .Join(_dbContext.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.HierarchyLevel)
            .DefaultIfEmpty(int.MaxValue)
            .MinAsync(cancellationToken);

        // Actor can only assign/remove roles with hierarchy level strictly GREATER than their own
        if (targetRoleHierarchyLevel <= actorHighestLevel)
        {
            return RoleResult.Failure(
                "You can only assign or remove roles with a lower privilege level than your own.", 403);
        }

        return RoleResult.Success();
    }

    private async Task<bool> IsUserSuperAdminAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        return await _userManager.IsInRoleAsync(user, "Super Admin");
    }

    private async Task LogAuditAsync(Guid actorUserId, string actionType, string targetEntity, string? targetEntityId,
        string? beforeJson, string? afterJson, CancellationToken cancellationToken)
    {
        var auditLog = new AuditLog
        {
            ActorUserId = actorUserId,
            ActionType = actionType,
            TargetEntity = targetEntity,
            TargetEntityId = targetEntityId,
            BeforeJson = beforeJson,
            AfterJson = afterJson,
            Timestamp = DateTime.UtcNow
        };

        _dbContext.AuditLogs.Add(auditLog);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static RoleResponse MapToRoleResponse(Role role)
    {
        return new RoleResponse(
            Id: role.Id,
            Name: role.Name ?? string.Empty,
            Description: role.Description,
            HierarchyLevel: role.HierarchyLevel,
            IsSystem: role.IsSystem,
            Permissions: role.RolePermissions.Select(rp => new PermissionResponse(
                rp.Permission.Id,
                rp.Permission.Name,
                rp.Permission.Description,
                rp.Permission.Module)).ToList(),
            UserCount: role.UserRoles.Count);
    }
}
