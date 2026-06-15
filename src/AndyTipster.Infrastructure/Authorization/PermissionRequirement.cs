using Microsoft.AspNetCore.Authorization;

namespace AndyTipster.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement that demands the user has a specific permission.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}
