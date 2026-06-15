namespace AndyTipster.Application.Roles.DTOs;

/// <summary>
/// Response DTO for role details.
/// </summary>
public record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    int HierarchyLevel,
    bool IsSystem,
    List<PermissionResponse> Permissions,
    int UserCount);

/// <summary>
/// Response DTO for permission details.
/// </summary>
public record PermissionResponse(
    Guid Id,
    string Name,
    string? Description,
    string Module);
