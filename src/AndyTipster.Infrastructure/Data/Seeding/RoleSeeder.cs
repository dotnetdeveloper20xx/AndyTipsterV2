using AndyTipster.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Data.Seeding;

/// <summary>
/// Seeds the 6 base roles, base permissions, and role-permission assignments.
/// </summary>
public static class RoleSeeder
{
    /// <summary>
    /// Seeds roles, permissions, and their assignments if they don't already exist.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AndyTipsterDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AndyTipsterDbContext>>();

        await SeedPermissionsAsync(dbContext, logger);
        await SeedRolesAsync(roleManager, logger);
        await SeedRolePermissionsAsync(dbContext, roleManager, logger);
    }

    private static async Task SeedPermissionsAsync(AndyTipsterDbContext dbContext, ILogger logger)
    {
        var permissionDefinitions = GetPermissionDefinitions();

        foreach (var (name, description, module) in permissionDefinitions)
        {
            var exists = await dbContext.Permissions.AnyAsync(p => p.Name == name);
            if (!exists)
            {
                dbContext.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    Module = module,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Permissions seeded successfully.");
    }

    private static async Task SeedRolesAsync(RoleManager<Role> roleManager, ILogger logger)
    {
        var roleDefinitions = GetRoleDefinitions();

        foreach (var (name, description, hierarchyLevel) in roleDefinitions)
        {
            if (!await roleManager.RoleExistsAsync(name))
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    NormalizedName = name.ToUpperInvariant(),
                    Description = description,
                    HierarchyLevel = hierarchyLevel,
                    IsSystem = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Seeded role: {RoleName} (Level {Level})", name, hierarchyLevel);
                }
                else
                {
                    logger.LogWarning("Failed to seed role {RoleName}: {Errors}", name,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedRolePermissionsAsync(AndyTipsterDbContext dbContext, RoleManager<Role> roleManager, ILogger logger)
    {
        var rolePermissionMappings = GetRolePermissionMappings();

        foreach (var (roleName, permissionNames) in rolePermissionMappings)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var existingPermissionIds = await dbContext.RolePermissions
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var permissions = await dbContext.Permissions
                .Where(p => permissionNames.Contains(p.Name))
                .ToListAsync();

            foreach (var permission in permissions)
            {
                if (!existingPermissionIds.Contains(permission.Id))
                {
                    dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        AssignedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Role-permission assignments seeded successfully.");
    }

    private static List<(string Name, string Description, string Module)> GetPermissionDefinitions()
    {
        return new List<(string, string, string)>
        {
            // Users
            ("Users.View", "View user profiles and listings", "Users"),
            ("Users.Create", "Create new user accounts", "Users"),
            ("Users.Edit", "Edit user profiles and settings", "Users"),
            ("Users.Delete", "Delete user accounts", "Users"),
            ("Users.Impersonate", "Impersonate other users", "Users"),

            // Roles
            ("Roles.View", "View roles and permissions", "Roles"),
            ("Roles.Create", "Create new roles", "Roles"),
            ("Roles.Edit", "Edit existing roles", "Roles"),
            ("Roles.Delete", "Delete roles", "Roles"),
            ("Roles.Assign", "Assign and remove roles from users", "Roles"),

            // Plans
            ("Plans.View", "View subscription plans", "Plans"),
            ("Plans.Create", "Create subscription plans", "Plans"),
            ("Plans.Edit", "Edit subscription plans", "Plans"),
            ("Plans.Delete", "Delete subscription plans", "Plans"),

            // Tips
            ("Tips.View", "View tips", "Tips"),
            ("Tips.Create", "Create new tips", "Tips"),
            ("Tips.Edit", "Edit existing tips", "Tips"),
            ("Tips.Delete", "Delete tips", "Tips"),

            // CMS
            ("CMS.View", "View CMS pages and content", "CMS"),
            ("CMS.Create", "Create CMS pages", "CMS"),
            ("CMS.Edit", "Edit CMS pages", "CMS"),
            ("CMS.Delete", "Delete CMS pages", "CMS"),
            ("CMS.Publish", "Publish and unpublish CMS pages", "CMS"),

            // Analytics
            ("Analytics.View", "View analytics dashboards", "Analytics"),

            // Subscriptions
            ("Subscriptions.View", "View subscription details", "Subscriptions"),
            ("Subscriptions.Manage", "Manage subscriptions (cancel, refund, etc.)", "Subscriptions"),

            // Audit
            ("Audit.View", "View audit logs", "Audit")
        };
    }

    private static List<(string Name, string Description, int HierarchyLevel)> GetRoleDefinitions()
    {
        return new List<(string, string, int)>
        {
            ("Super Admin", "Full system access with all permissions", 1),
            ("Admin", "Administrative access for managing users, content, and subscriptions", 2),
            ("Moderator", "Content moderation and community management", 3),
            ("Subscriber", "Paid subscriber with access to premium tips", 4),
            ("Free User", "Default role for newly registered users", 5),
            ("Guest", "Unauthenticated visitor with minimal access", 6)
        };
    }

    private static Dictionary<string, List<string>> GetRolePermissionMappings()
    {
        return new Dictionary<string, List<string>>
        {
            ["Super Admin"] = new List<string>
            {
                "Users.View", "Users.Create", "Users.Edit", "Users.Delete", "Users.Impersonate",
                "Roles.View", "Roles.Create", "Roles.Edit", "Roles.Delete", "Roles.Assign",
                "Plans.View", "Plans.Create", "Plans.Edit", "Plans.Delete",
                "Tips.View", "Tips.Create", "Tips.Edit", "Tips.Delete",
                "CMS.View", "CMS.Create", "CMS.Edit", "CMS.Delete", "CMS.Publish",
                "Analytics.View",
                "Subscriptions.View", "Subscriptions.Manage",
                "Audit.View"
            },
            ["Admin"] = new List<string>
            {
                "Users.View", "Users.Create", "Users.Edit", "Users.Delete",
                "Roles.View", "Roles.Assign",
                "Plans.View", "Plans.Create", "Plans.Edit", "Plans.Delete",
                "Tips.View", "Tips.Create", "Tips.Edit", "Tips.Delete",
                "CMS.View", "CMS.Create", "CMS.Edit", "CMS.Delete", "CMS.Publish",
                "Analytics.View",
                "Subscriptions.View", "Subscriptions.Manage",
                "Audit.View"
            },
            ["Moderator"] = new List<string>
            {
                "Users.View",
                "Tips.View", "Tips.Edit",
                "CMS.View", "CMS.Edit",
                "Analytics.View"
            },
            ["Subscriber"] = new List<string>
            {
                "Tips.View",
                "Subscriptions.View"
            },
            ["Free User"] = new List<string>
            {
                "Tips.View"
            },
            ["Guest"] = new List<string>()
        };
    }
}
