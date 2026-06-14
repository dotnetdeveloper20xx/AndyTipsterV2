using Microsoft.AspNetCore.Identity;

namespace AndyTipster.Domain.Entities;

public class UserRole : IdentityUserRole<Guid>
{
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedByUserId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
