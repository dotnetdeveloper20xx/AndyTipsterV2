namespace AndyTipster.Domain.Entities;

public class GdprConsent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
