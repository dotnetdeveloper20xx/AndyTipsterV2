using Microsoft.AspNetCore.Identity;

namespace AndyTipster.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? TimeZone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletionRequestedAt { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Referral> Referrals { get; set; } = new List<Referral>();
    public ICollection<GdprConsent> GdprConsents { get; set; } = new List<GdprConsent>();
}
