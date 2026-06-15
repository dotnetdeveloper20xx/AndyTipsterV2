using System.Text;
using AndyTipster.Application.Audit.Services;
using AndyTipster.Application.Users.DTOs;
using AndyTipster.Application.Users.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Configuration;
using AndyTipster.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Implements admin user management: paginated listing, search/filter, impersonation,
/// bulk actions, suspension, and CSV export.
/// </summary>
public class UserManagementService : IUserManagementService
{
    private readonly AndyTipsterDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;
    private readonly JwtSettings _jwtSettings;

    public UserManagementService(
        AndyTipsterDbContext context,
        UserManager<ApplicationUser> userManager,
        IAuditService auditService,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _userManager = userManager;
        _auditService = auditService;
        _jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public async Task<UserListResponse> GetUsersAsync(UserListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Subscriptions)
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .AsQueryable();

        // Apply search (name or email)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.DisplayName.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search));
        }

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
        {
            query = query.Where(u =>
                u.UserRoles.Any(ur => ur.Role.Name == request.RoleFilter));
        }

        // Apply plan filter
        if (!string.IsNullOrWhiteSpace(request.PlanFilter))
        {
            query = query.Where(u =>
                u.Subscriptions.Any(s =>
                    s.Plan.Name == request.PlanFilter &&
                    (s.Status == Domain.Enumerations.SubscriptionStatus.Active || s.Status == Domain.Enumerations.SubscriptionStatus.Trialing)));
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.StatusFilter))
        {
            query = request.StatusFilter.ToLower() switch
            {
                "active" => query.Where(u => !u.IsSuspended && u.EmailConfirmed),
                "suspended" => query.Where(u => u.IsSuspended),
                "unverified" => query.Where(u => !u.EmailConfirmed),
                _ => query
            };
        }

        // Apply date range filters
        if (request.RegisteredFrom.HasValue)
            query = query.Where(u => u.CreatedAt >= request.RegisteredFrom.Value);
        if (request.RegisteredTo.HasValue)
            query = query.Where(u => u.CreatedAt <= request.RegisteredTo.Value);
        if (request.LastLoginFrom.HasValue)
            query = query.Where(u => u.LastLoginAt >= request.LastLoginFrom.Value);
        if (request.LastLoginTo.HasValue)
            query = query.Where(u => u.LastLoginAt <= request.LastLoginTo.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "displayname" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(u => u.DisplayName)
                : query.OrderByDescending(u => u.DisplayName),
            "email" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(u => u.Email)
                : query.OrderByDescending(u => u.Email),
            "lastloginat" => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(u => u.LastLoginAt)
                : query.OrderByDescending(u => u.LastLoginAt),
            _ => request.SortDirection?.ToLower() == "asc"
                ? query.OrderBy(u => u.CreatedAt)
                : query.OrderByDescending(u => u.CreatedAt),
        };

        // Apply pagination
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(request.Page, 1);

        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserSummaryDto
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email!,
                Roles = u.UserRoles.Select(ur => ur.Role.Name!).ToList(),
                Status = u.IsSuspended ? "Suspended" : (!u.EmailConfirmed ? "Unverified" : "Active"),
                Plan = u.Subscriptions
                    .Where(s => s.Status == Domain.Enumerations.SubscriptionStatus.Active || s.Status == Domain.Enumerations.SubscriptionStatus.Trialing)
                    .Select(s => s.Plan.Name)
                    .FirstOrDefault(),
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                AvatarUrl = u.AvatarUrl
            })
            .ToListAsync(cancellationToken);

        return new UserListResponse
        {
            Users = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    /// <inheritdoc />
    public async Task<UserDetailResponse?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.Plan)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user is null)
            return null;

        return new UserDetailResponse
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email!,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            TimeZone = user.TimeZone,
            Roles = user.UserRoles.Select(ur => ur.Role.Name!).ToList(),
            Status = user.IsSuspended ? "Suspended" : (!user.EmailConfirmed ? "Unverified" : "Active"),
            Plan = user.Subscriptions
                .Where(s => s.Status == Domain.Enumerations.SubscriptionStatus.Active || s.Status == Domain.Enumerations.SubscriptionStatus.Trialing)
                .Select(s => s.Plan.Name)
                .FirstOrDefault(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsSuspended = user.IsSuspended,
            EmailConfirmed = user.EmailConfirmed,
            TwoFactorEnabled = user.TwoFactorEnabled
        };
    }

    /// <inheritdoc />
    public async Task<ImpersonateResponse?> ImpersonateUserAsync(Guid userId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user is null)
            return null;

        // Generate a limited read-only impersonation token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.DisplayName),
            new("impersonator_id", actorUserId.ToString()),
            new("is_impersonation", "true"),
            new("read_only", "true")
        };

        // Add user roles as claims
        foreach (var userRole in user.UserRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name!));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(30); // Impersonation sessions last 30 minutes

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Log impersonation action
        await _auditService.LogActionAsync(
            actorUserId,
            "UserImpersonation",
            "User",
            userId.ToString(),
            cancellationToken: cancellationToken);

        return new ImpersonateResponse
        {
            ImpersonationToken = tokenString,
            ImpersonatedUserId = user.Id,
            ImpersonatedUserName = user.DisplayName,
            ImpersonatedUserEmail = user.Email!,
            ExpiresAt = expiresAt
        };
    }

    /// <inheritdoc />
    public async Task<BulkActionResponse> ExecuteBulkActionAsync(BulkActionRequest request, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var response = new BulkActionResponse
        {
            TotalRequested = request.UserIds.Count
        };

        foreach (var userId in request.UserIds)
        {
            try
            {
                switch (request.Action.ToLower())
                {
                    case "suspend":
                        var suspended = await SuspendUserAsync(userId, actorUserId, cancellationToken);
                        if (suspended) response.Succeeded++;
                        else
                        {
                            response.Failed++;
                            response.Failures.Add(new BulkActionFailure { UserId = userId, Reason = "User not found or already suspended." });
                        }
                        break;

                    case "role_change":
                        if (string.IsNullOrWhiteSpace(request.RoleName))
                        {
                            response.Failed++;
                            response.Failures.Add(new BulkActionFailure { UserId = userId, Reason = "Role name is required for role change action." });
                            continue;
                        }
                        var roleChanged = await ChangeUserRoleAsync(userId, request.RoleName, actorUserId, cancellationToken);
                        if (roleChanged) response.Succeeded++;
                        else
                        {
                            response.Failed++;
                            response.Failures.Add(new BulkActionFailure { UserId = userId, Reason = "Failed to change role. User or role not found." });
                        }
                        break;

                    default:
                        response.Failed++;
                        response.Failures.Add(new BulkActionFailure { UserId = userId, Reason = $"Unknown action: {request.Action}" });
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Failed++;
                response.Failures.Add(new BulkActionFailure { UserId = userId, Reason = ex.Message });
            }
        }

        return response;
    }

    /// <inheritdoc />
    public async Task<bool> SuspendUserAsync(Guid userId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user is null || user.IsSuspended)
            return false;

        var beforeState = JsonSerializer.Serialize(new { user.IsSuspended });

        // Suspend the user
        user.IsSuspended = true;

        // Revoke all active refresh tokens within 5 seconds
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.RevokedAt.HasValue && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var afterState = JsonSerializer.Serialize(new { user.IsSuspended });

        // Log suspension in audit trail
        await _auditService.LogActionAsync(
            actorUserId,
            "UserSuspended",
            "User",
            userId.ToString(),
            beforeState,
            afterState,
            cancellationToken: cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportUsersAsync(UserListRequest request, CancellationToken cancellationToken = default)
    {
        // Use the same filtering logic but without pagination limits
        var query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.Subscriptions)
                .ThenInclude(s => s.Plan)
            .Where(u => !u.IsDeleted)
            .AsNoTracking()
            .AsQueryable();

        // Apply search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.DisplayName.ToLower().Contains(search) ||
                u.Email!.ToLower().Contains(search));
        }

        // Apply role filter
        if (!string.IsNullOrWhiteSpace(request.RoleFilter))
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == request.RoleFilter));

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(request.StatusFilter))
        {
            query = request.StatusFilter.ToLower() switch
            {
                "active" => query.Where(u => !u.IsSuspended && u.EmailConfirmed),
                "suspended" => query.Where(u => u.IsSuspended),
                "unverified" => query.Where(u => !u.EmailConfirmed),
                _ => query
            };
        }

        // Apply date range filters
        if (request.RegisteredFrom.HasValue)
            query = query.Where(u => u.CreatedAt >= request.RegisteredFrom.Value);
        if (request.RegisteredTo.HasValue)
            query = query.Where(u => u.CreatedAt <= request.RegisteredTo.Value);

        // Default sort by registration date descending
        query = query.OrderByDescending(u => u.CreatedAt);

        var users = await query
            .Take(100_000)
            .Select(u => new
            {
                u.DisplayName,
                Email = u.Email!,
                Roles = string.Join("; ", u.UserRoles.Select(ur => ur.Role.Name)),
                Status = u.IsSuspended ? "Suspended" : (!u.EmailConfirmed ? "Unverified" : "Active"),
                Plan = u.Subscriptions
                    .Where(s => s.Status == Domain.Enumerations.SubscriptionStatus.Active || s.Status == Domain.Enumerations.SubscriptionStatus.Trialing)
                    .Select(s => s.Plan.Name)
                    .FirstOrDefault() ?? "",
                u.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Generate CSV
        var sb = new StringBuilder();
        sb.AppendLine("Name,Email,Roles,Status,Plan,Registration Date");

        foreach (var user in users)
        {
            sb.AppendLine($"\"{EscapeCsv(user.DisplayName)}\",\"{EscapeCsv(user.Email)}\",\"{EscapeCsv(user.Roles)}\",\"{user.Status}\",\"{EscapeCsv(user.Plan)}\",\"{user.CreatedAt:yyyy-MM-dd HH:mm:ss}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private async Task<bool> ChangeUserRoleAsync(Guid userId, string roleName, Guid actorUserId, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null || user.IsDeleted) return false;

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
        if (role is null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        var beforeState = JsonSerializer.Serialize(new { Roles = currentRoles });

        // Remove current roles and assign new one
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        var result = await _userManager.AddToRoleAsync(user, roleName);

        if (!result.Succeeded) return false;

        var afterState = JsonSerializer.Serialize(new { Roles = new[] { roleName } });

        await _auditService.LogActionAsync(
            actorUserId,
            "BulkRoleChange",
            "User",
            userId.ToString(),
            beforeState,
            afterState,
            cancellationToken: cancellationToken);

        return true;
    }

    private static string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }
}
