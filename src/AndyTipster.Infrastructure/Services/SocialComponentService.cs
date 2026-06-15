using AndyTipster.Application.Social.DTOs;
using AndyTipster.Application.Social.Services;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class SocialComponentService : ISocialComponentService
{
    private readonly AndyTipsterDbContext _context;
    private readonly ILogger<SocialComponentService> _logger;

    // In-memory store for social links and visibility
    private static List<SocialLinkDto> _socialLinks = new()
    {
        new() { Platform = "Twitter", Url = "https://twitter.com/andytipster", Label = "Follow on X", IsVisible = true },
        new() { Platform = "Facebook", Url = "https://facebook.com/andytipster", Label = "Like on Facebook", IsVisible = true },
        new() { Platform = "Instagram", Url = "https://instagram.com/andytipster", Label = "Follow on Instagram", IsVisible = true },
        new() { Platform = "Telegram", Url = "https://t.me/andytipster", Label = "Join Telegram", IsVisible = true },
        new() { Platform = "YouTube", Url = "https://youtube.com/@andytipster", Label = "Subscribe on YouTube", IsVisible = true }
    };

    private static readonly Dictionary<string, List<SocialComponentVisibilityDto>> _pageVisibility = new();
    private static readonly object _lock = new();

    public SocialComponentService(AndyTipsterDbContext context, ILogger<SocialComponentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task<SocialFollowBarDto> GetSocialFollowBarAsync()
    {
        return Task.FromResult(new SocialFollowBarDto
        {
            Links = _socialLinks.Where(l => l.IsVisible).ToList(),
            IsVisible = true
        });
    }

    public Task UpdateSocialLinksAsync(UpdateSocialLinksDto dto)
    {
        lock (_lock)
        {
            _socialLinks = dto.Links.ToList();
        }

        _logger.LogInformation("Social links updated with {Count} links", dto.Links.Count);
        return Task.CompletedTask;
    }

    public async Task<OpenGraphMetaDto> GetOpenGraphMetaAsync(string pageSlug)
    {
        // Try to find page SEO metadata
        var page = await _context.CmsPages
            .FirstOrDefaultAsync(p => p.Slug == pageSlug);

        if (page != null)
        {
            return new OpenGraphMetaDto
            {
                Title = page.MetaTitle ?? page.Title,
                Description = page.MetaDescription ?? "",
                Url = $"https://andytipster.com/{pageSlug}",
                ImageUrl = null,
                SiteName = "AndyTipster",
                Type = "website"
            };
        }

        return new OpenGraphMetaDto
        {
            Title = "AndyTipster - Premium Horse Racing Tips",
            Description = "Get daily expert horse racing tips from a proven tipster.",
            Url = $"https://andytipster.com/{pageSlug}",
            SiteName = "AndyTipster",
            Type = "website"
        };
    }

    public async Task<SocialProofDto> GetSocialProofAsync()
    {
        var subscriberCount = await _context.Subscriptions
            .Where(s => s.Status == Domain.Enumerations.SubscriptionStatus.Active)
            .CountAsync();

        var totalTips = await _context.Tips.CountAsync();

        var wonTips = await _context.Tips
            .CountAsync(t => t.Result == Domain.Enumerations.TipResult.Won);

        var settledTips = await _context.Tips
            .CountAsync(t => t.Result != null);

        var winRate = settledTips > 0 ? Math.Round((double)wonTips / settledTips * 100, 1) : 0;

        return new SocialProofDto
        {
            SubscriberCount = subscriberCount,
            TipsDelivered = totalTips,
            WinRate = winRate
        };
    }

    public async Task<ShareDialogDto> GetShareDialogAsync(string pageSlug)
    {
        var meta = await GetOpenGraphMetaAsync(pageSlug);

        return new ShareDialogDto
        {
            Url = meta.Url,
            Title = meta.Title,
            Description = meta.Description,
            ImageUrl = meta.ImageUrl
        };
    }

    public Task SetComponentVisibilityAsync(SocialComponentVisibilityDto dto)
    {
        lock (_lock)
        {
            if (!_pageVisibility.ContainsKey(dto.PageSlug))
                _pageVisibility[dto.PageSlug] = new List<SocialComponentVisibilityDto>();

            var existing = _pageVisibility[dto.PageSlug]
                .FirstOrDefault(v => v.ComponentType == dto.ComponentType);

            if (existing != null)
                _pageVisibility[dto.PageSlug].Remove(existing);

            _pageVisibility[dto.PageSlug].Add(dto);
        }

        return Task.CompletedTask;
    }

    public Task<List<SocialComponentVisibilityDto>> GetPageComponentVisibilityAsync(string pageSlug)
    {
        lock (_lock)
        {
            if (_pageVisibility.TryGetValue(pageSlug, out var visibility))
                return Task.FromResult(visibility);

            return Task.FromResult(new List<SocialComponentVisibilityDto>());
        }
    }
}
