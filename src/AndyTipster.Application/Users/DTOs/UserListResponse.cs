namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Paginated response DTO for user listing.
/// </summary>
public class UserListResponse
{
    public List<UserSummaryDto> Users { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UserSummaryDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public string? Plan { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? AvatarUrl { get; set; }
}
