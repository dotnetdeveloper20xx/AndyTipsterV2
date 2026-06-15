using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AndyTipster.Application.Auth.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Configuration;
using AndyTipster.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Implements JWT access token generation and refresh token management with rotation.
/// </summary>
public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AndyTipsterDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public TokenService(
        UserManager<ApplicationUser> userManager,
        AndyTipsterDbContext dbContext,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<(string Token, DateTime ExpiresAt)> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("display_name", user.DisplayName)
        };

        // Add role claims
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permission claims from roles
        var permissions = await GetUserPermissionsAsync(user.Id);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, expiresAt);
    }

    public async Task<string> GenerateRefreshTokenAsync(ApplicationUser user, string? ipAddress = null)
    {
        var tokenValue = GenerateSecureRandomToken();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync();

        return tokenValue;
    }

    public async Task<(ApplicationUser? User, RefreshToken? Token)> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken is null)
        {
            return (null, null);
        }

        if (!refreshToken.IsActive)
        {
            return (null, refreshToken);
        }

        return (refreshToken.User, refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string token, string? ipAddress = null, string? replacedByToken = null)
    {
        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken is null)
        {
            return;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReplacedByToken = replacedByToken;

        await _dbContext.SaveChangesAsync();
    }

    private async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
    {
        // Get permissions through user's roles
        var permissions = await _dbContext.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => _dbContext.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .Contains(rp.RoleId))
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToListAsync();

        return permissions;
    }

    private static string GenerateSecureRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
