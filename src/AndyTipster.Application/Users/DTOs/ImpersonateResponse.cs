namespace AndyTipster.Application.Users.DTOs;

/// <summary>
/// Response DTO for user impersonation.
/// </summary>
public class ImpersonateResponse
{
    public string ImpersonationToken { get; set; } = string.Empty;
    public Guid ImpersonatedUserId { get; set; }
    public string ImpersonatedUserName { get; set; } = string.Empty;
    public string ImpersonatedUserEmail { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
