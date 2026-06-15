namespace AndyTipster.Application.Referral.DTOs;

public record ReferralLinkDto
{
    public string ReferralCode { get; init; } = string.Empty;
    public string ReferralUrl { get; init; } = string.Empty;
}

public record ReferralDashboardDto
{
    public string ReferralCode { get; init; } = string.Empty;
    public string ReferralUrl { get; init; } = string.Empty;
    public int TotalClicks { get; init; }
    public int TotalConversions { get; init; }
    public decimal TotalRewardsEarned { get; init; }
    public List<ReferralItemDto> Referrals { get; init; } = new();
}

public record ReferralItemDto
{
    public Guid Id { get; init; }
    public string? ReferredEmail { get; init; }
    public bool IsConverted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ConvertedAt { get; init; }
}

public record ReferralConfigDto
{
    public decimal RewardAmount { get; init; }
    public string RewardType { get; init; } = "discount"; // discount or credit
    public int MaxReferralsPerUser { get; init; } = 50;
    public bool IsActive { get; init; } = true;
}

public record UpdateReferralConfigDto
{
    public decimal? RewardAmount { get; init; }
    public string? RewardType { get; init; }
    public int? MaxReferralsPerUser { get; init; }
    public bool? IsActive { get; init; }
}

public record TrackReferralClickDto
{
    public string ReferralCode { get; init; } = string.Empty;
}

public record ConvertReferralDto
{
    public string ReferralCode { get; init; } = string.Empty;
    public Guid ReferredUserId { get; init; }
}
