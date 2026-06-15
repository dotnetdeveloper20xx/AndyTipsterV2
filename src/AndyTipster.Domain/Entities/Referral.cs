namespace AndyTipster.Domain.Entities;

public class Referral
{
    public Guid Id { get; set; }
    public Guid ReferrerUserId { get; set; }
    public Guid? ReferredUserId { get; set; }
    public string ReferralCode { get; set; } = string.Empty;
    public string? ReferredEmail { get; set; }
    public bool IsConverted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConvertedAt { get; set; }

    public ApplicationUser ReferrerUser { get; set; } = null!;
    public ApplicationUser? ReferredUser { get; set; }
}
