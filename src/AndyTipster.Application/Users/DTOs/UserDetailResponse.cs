namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Detailed user information response DTO.
/// </summary>
public class UserDetailResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? TimeZone { get; set; }
    public List<string> Roles { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string? Plan { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsSuspended { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
}
