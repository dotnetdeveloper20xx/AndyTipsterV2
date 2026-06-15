using Microsoft.AspNetCore.Identity;

namespace AndyTipster.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    public string? Description { get; set; }
    public int HierarchyLevel { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
