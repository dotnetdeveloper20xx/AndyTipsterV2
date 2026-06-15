using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace AndyTipster.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks if the current user has the required permission claim.
/// Permissions are added to the JWT token as claims during token generation.
/// </summary>
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Check if user has the permission claim
        var permissionClaim = context.User.FindAll("permission")
            .Any(c => c.Value == requirement.Permission);

        if (permissionClaim)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
