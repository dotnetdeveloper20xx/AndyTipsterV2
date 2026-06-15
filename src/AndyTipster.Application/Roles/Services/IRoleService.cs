using AndyTipster.Application.Roles.DTOs;

namespace AndyTipster.Application.Roles.Services;

/// <summary>
/// Service interface for role and permission management operations.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Gets all roles with permission and user count details.
    /// </summary>
    Task<List<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by its ID with full permission details.
    /// </summary>
    Task<RoleResult<RoleResponse>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a custom role with the specified permissions. Only Super Admin can create roles.
    /// </summary>
    Task<RoleResult<RoleResponse>> CreateRoleAsync(CreateRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing role's name, description, hierarchy level, and permissions.
    /// </summary>
    Task<RoleResult<RoleResponse>> UpdateRoleAsync(Guid roleId, UpdateRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role. Rejects if role is a system role or has users assigned.
    /// </summary>
    Task<RoleResult> DeleteRoleAsync(Guid roleId, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to a user. Enforces role hierarchy (actor can only assign roles below own level).
    /// </summary>
    Task<RoleResult> AssignRoleToUserAsync(AssignRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a role from a user. Enforces role hierarchy.
    /// </summary>
    Task<RoleResult> RemoveRoleFromUserAsync(RemoveRoleRequest request, Guid actorUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the union of all permissions across all roles assigned to a user.
    /// </summary>
    Task<RoleResult<List<PermissionResponse>>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available permissions.
    /// </summary>
    Task<List<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Result type for role operations that return data.
/// </summary>
public record RoleResult<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; } = 200;

    public static RoleResult<T> Success(T data, int statusCode = 200) => new()
    {
        Succeeded = true,
        Data = data,
        StatusCode = statusCode
    };

    public static RoleResult<T> Failure(string error, int statusCode = 400) => new()
    {
        Succeeded = false,
        ErrorMessage = error,
        StatusCode = statusCode
    };
}

/// <summary>
/// Result type for role operations that don't return data.
/// </summary>
public record RoleResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public int StatusCode { get; init; } = 200;

    public static RoleResult Success(int statusCode = 200) => new()
    {
        Succeeded = true,
        StatusCode = statusCode
    };

    public static RoleResult Failure(string error, int statusCode = 400) => new()
    {
        Succeeded = false,
        ErrorMessage = error,
        StatusCode = statusCode
    };
}
