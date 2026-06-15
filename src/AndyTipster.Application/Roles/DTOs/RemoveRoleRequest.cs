namespace AndyTipster.Application.Roles.DTOs;

/// <summary>
/// Request DTO for removing a role from a user.
/// </summary>
public record RemoveRoleRequest(
    Guid UserId,
    Guid RoleId);
