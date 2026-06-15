namespace AndyTipster.Application.Roles.DTOs;

/// <summary>
/// Request DTO for assigning a role to a user.
/// </summary>
public record AssignRoleRequest(
    Guid UserId,
    Guid RoleId);
