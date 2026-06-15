namespace AndyTipster.Application.Roles.DTOs;

/// <summary>
/// Request DTO for creating a custom role.
/// </summary>
public record CreateRoleRequest(
    string RoleName,
    string? Description,
    int HierarchyLevel,
    List<Guid> PermissionIds);
