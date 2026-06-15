using AndyTipster.Application.Referral.DTOs;
using AndyTipster.Application.Referral.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Domain.Exceptions;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class ReferralService : IReferralService
{
    private readonly AndyTipsterDbContext _context;
    private readonly ILogger<ReferralService> _logger;

    // In-memory referral config (would be stored in DB in production)
    private static ReferralConfigDto _config = new()
    {
        RewardAmount = 10.00m,
        RewardType = "discount",
        MaxReferralsPerUser = 50,
        IsActive = true
    };

    // Track clicks per referral code
    private static readonly Dictionary<string, int> _clickCounts = new();
    private static readonly object _lock = new();

    public ReferralService(AndyTipsterDbContext context, ILogger<ReferralService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReferralLinkDto> GetOrCreateReferralLinkAsync(Guid userId)
    {
        // Check user has active subscription
        var hasSubscription = await _context.Subscriptions
            .AnyAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);

        if (!hasSubscription)
            throw new BusinessRuleException("Only active subscribers can generate referral links.");

        var existingReferral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferrerUserId == userId && r.ReferredUserId == null && !r.IsConverted);

        string referralCode;
        if (existingReferral != null)
        {
            referralCode = existingReferral.ReferralCode;
        }
        else
        {
            referralCode = GenerateReferralCode();
            var referral = new Domain.Entities.Referral
            {
                Id = Guid.NewGuid(),
                ReferrerUserId = userId,
                ReferralCode = referralCode,
                IsConverted = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Referrals.Add(referral);
            await _context.SaveChangesAsync();
        }

        return new ReferralLinkDto
        {
            ReferralCode = referralCode,
            ReferralUrl = $"https://andytipster.com/ref/{referralCode}"
        };
    }

    public async Task<ReferralDashboardDto> GetDashboardAsync(Guid userId)
    {
        var referrals = await _context.Referrals
            .Where(r => r.ReferrerUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var referralCode = referrals.FirstOrDefault()?.ReferralCode ?? "";

        int totalClicks;
        lock (_lock)
        {
            _clickCounts.TryGetValue(referralCode, out totalClicks);
        }

        var conversions = referrals.Count(r => r.IsConverted);
        var rewardsEarned = conversions * _config.RewardAmount;

        return new ReferralDashboardDto
        {
            ReferralCode = referralCode,
            ReferralUrl = $"https://andytipster.com/ref/{referralCode}",
            TotalClicks = totalClicks,
            TotalConversions = conversions,
            TotalRewardsEarned = rewardsEarned,
            Referrals = referrals.Select(r => new ReferralItemDto
            {
                Id = r.Id,
                ReferredEmail = r.ReferredEmail,
                IsConverted = r.IsConverted,
                CreatedAt = r.CreatedAt,
                ConvertedAt = r.ConvertedAt
            }).ToList()
        };
    }

    public Task TrackClickAsync(string referralCode)
    {
        lock (_lock)
        {
            if (_clickCounts.ContainsKey(referralCode))
                _clickCounts[referralCode]++;
            else
                _clickCounts[referralCode] = 1;
        }

        _logger.LogInformation("Referral click tracked for code {Code}", referralCode);
        return Task.CompletedTask;
    }

    public async Task<bool> ConvertReferralAsync(ConvertReferralDto dto)
    {
        if (!_config.IsActive)
            return false;

        var referral = await _context.Referrals
            .FirstOrDefaultAsync(r => r.ReferralCode == dto.ReferralCode && !r.IsConverted);

        if (referral == null)
            return false;

        // Check max referrals limit
        var existingConversions = await _context.Referrals
            .CountAsync(r => r.ReferrerUserId == referral.ReferrerUserId && r.IsConverted);

        if (existingConversions >= _config.MaxReferralsPerUser)
            return false;

        referral.ReferredUserId = dto.ReferredUserId;
        referral.IsConverted = true;
        referral.ConvertedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Referral converted: code {Code}, referrer {ReferrerId}, referred {ReferredId}",
            dto.ReferralCode, referral.ReferrerUserId, dto.ReferredUserId);

        return true;
    }

    public Task<ReferralConfigDto> GetConfigAsync()
    {
        return Task.FromResult(_config);
    }

    public Task<ReferralConfigDto> UpdateConfigAsync(UpdateReferralConfigDto dto)
    {
        _config = _config with
        {
            RewardAmount = dto.RewardAmount ?? _config.RewardAmount,
            RewardType = dto.RewardType ?? _config.RewardType,
            MaxReferralsPerUser = dto.MaxReferralsPerUser ?? _config.MaxReferralsPerUser,
            IsActive = dto.IsActive ?? _config.IsActive
        };

        _logger.LogInformation("Referral config updated: reward={Amount}, type={Type}, max={Max}, active={Active}",
            _config.RewardAmount, _config.RewardType, _config.MaxReferralsPerUser, _config.IsActive);

        return Task.FromResult(_config);
    }

    private static string GenerateReferralCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
    }
}
