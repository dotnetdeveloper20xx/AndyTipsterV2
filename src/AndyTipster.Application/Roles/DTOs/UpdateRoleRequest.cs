namespace AndyTipster.Application.Roles.DTOs;

/// <summary>
/// Request DTO for updating an existing role.
/// </summary>
public record UpdateRoleRequest(
    string RoleName,
    string? Description,
    int HierarchyLevel,
    List<Guid> PermissionIds);
