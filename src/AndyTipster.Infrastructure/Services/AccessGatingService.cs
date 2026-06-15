using AndyTipster.Application.Tips.DTOs;
using AndyTipster.Application.Tips.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class AccessGatingService : IAccessGatingService
{
    private readonly AndyTipsterDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccessGatingService(AndyTipsterDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AccessCheckResult> CheckAccessAsync(Guid userId, Guid tipId)
    {
        var tip = await _context.Tips.Include(t => t.Category).FirstOrDefaultAsync(t => t.Id == tipId);
        if (tip == null)
            return new AccessCheckResult { HasAccess = false, DenialReason = "Tip not found." };

        if (tip.Status != TipStatus.Published)
            return new AccessCheckResult { HasAccess = false, DenialReason = "Tip is not published." };

        // Check if this is tip of the day (most recent published tip)
        var tipOfTheDay = await GetTipOfTheDayInternalAsync();
        var isTipOfTheDay = tipOfTheDay != null && tipOfTheDay.Id == tipId;

        // Guest user (no userId)
        if (userId == Guid.Empty)
        {
            if (isTipOfTheDay)
                return new AccessCheckResult { HasAccess = false, ShowPaywall = true, IsTipOfTheDay = true };

            return new AccessCheckResult { HasAccess = false, ShowPaywall = true, DenialReason = "Please subscribe to access tips." };
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new AccessCheckResult { HasAccess = false, DenialReason = "User not found." };

        var userRoles = await _userManager.GetRolesAsync(user);

        // Admin/Moderator always have access
        if (userRoles.Any(r => r is "Super Admin" or "Admin" or "Moderator"))
            return new AccessCheckResult { HasAccess = true };

        // Free User gets Tip of the Day
        if (userRoles.Contains("Free User"))
        {
            if (isTipOfTheDay)
                return new AccessCheckResult { HasAccess = true, IsTipOfTheDay = true };

            return new AccessCheckResult { HasAccess = false, ShowPaywall = true, DenialReason = "Upgrade your plan to access all tips." };
        }

        // Subscriber - check active subscription with matching category
        return await CheckSubscriberAccessAsync(userId, tip.CategoryId);
    }

    public async Task<AccessCheckResult> CheckCategoryAccessAsync(Guid userId, Guid categoryId)
    {
        if (userId == Guid.Empty)
            return new AccessCheckResult { HasAccess = false, ShowPaywall = true, DenialReason = "Please subscribe to access tips." };

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return new AccessCheckResult { HasAccess = false, DenialReason = "User not found." };

        var userRoles = await _userManager.GetRolesAsync(user);

        if (userRoles.Any(r => r is "Super Admin" or "Admin" or "Moderator"))
            return new AccessCheckResult { HasAccess = true };

        return await CheckSubscriberAccessAsync(userId, categoryId);
    }

    public async Task<TipDto?> GetTipOfTheDayAsync()
    {
        var tip = await GetTipOfTheDayInternalAsync();
        if (tip == null) return null;

        return new TipDto
        {
            Id = tip.Id,
            EventDate = tip.EventDate,
            RaceName = tip.RaceName,
            Selection = tip.Selection,
            Odds = tip.Odds,
            Stake = tip.Stake,
            CategoryId = tip.CategoryId,
            CategoryName = tip.Category?.Name ?? "",
            Commentary = tip.Commentary,
            Status = tip.Status.ToString(),
            Result = tip.Result?.ToString(),
            ProfitLoss = tip.ProfitLoss,
            CreatedAt = tip.CreatedAt,
            PublishedAt = tip.PublishedAt,
            ScheduledPublishAt = tip.ScheduledPublishAt,
            CreatedByUserId = tip.CreatedByUserId
        };
    }

    public async Task<(List<TipDto> Items, int TotalCount)> GetAccessibleTipsAsync(Guid userId, TipFilterDto filter)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var userRoles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();

        var query = _context.Tips
            .Include(t => t.Category)
            .Where(t => t.Status == TipStatus.Published)
            .AsQueryable();

        // Apply filters
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.EventDate >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            query = query.Where(t => t.EventDate <= filter.EndDate.Value);
        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        if (!string.IsNullOrEmpty(filter.Result) && Enum.TryParse<TipResult>(filter.Result, true, out var result))
            query = query.Where(t => t.Result == result);

        // Admin/Mod see all
        if (!userRoles.Any(r => r is "Super Admin" or "Admin" or "Moderator"))
        {
            // Get accessible category IDs for this subscriber
            var accessibleCategoryIds = await GetAccessibleCategoryIdsAsync(userId);
            if (accessibleCategoryIds.Count > 0)
                query = query.Where(t => accessibleCategoryIds.Contains(t.CategoryId));
            else
                query = query.Where(t => false); // No access
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.PublishedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var dtos = items.Select(t => new TipDto
        {
            Id = t.Id,
            EventDate = t.EventDate,
            RaceName = t.RaceName,
            Selection = t.Selection,
            Odds = t.Odds,
            Stake = t.Stake,
            CategoryId = t.CategoryId,
            CategoryName = t.Category.Name,
            Commentary = t.Commentary,
            Status = t.Status.ToString(),
            Result = t.Result?.ToString(),
            ProfitLoss = t.ProfitLoss,
            CreatedAt = t.CreatedAt,
            PublishedAt = t.PublishedAt,
            ScheduledPublishAt = t.ScheduledPublishAt,
            CreatedByUserId = t.CreatedByUserId
        }).ToList();

        return (dtos, totalCount);
    }

    private async Task<AccessCheckResult> CheckSubscriberAccessAsync(Guid userId, Guid categoryId)
    {
        // Get active subscriptions for this user
        var activeSubscription = await _context.Subscriptions
            .Include(s => s.Plan)
                .ThenInclude(p => p.IncludedCategories)
            .Where(s => s.UserId == userId &&
                (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (activeSubscription == null)
        {
            // Check if they have a past-due subscription still within grace period
            var pastDueSubscription = await _context.Subscriptions
                .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.PastDue)
                .FirstOrDefaultAsync();

            if (pastDueSubscription != null && pastDueSubscription.GracePeriodEndsAt.HasValue &&
                pastDueSubscription.GracePeriodEndsAt.Value > DateTime.UtcNow)
            {
                // Still within grace period - allow access
                return new AccessCheckResult { HasAccess = true };
            }

            return new AccessCheckResult
            {
                HasAccess = false,
                ShowPaywall = true,
                DenialReason = "Your subscription has expired or payment has failed. Please update your payment method."
            };
        }

        // Check if the plan includes the requested category
        var planIncludesCategory = activeSubscription.Plan.IncludedCategories
            .Any(c => c.Id == categoryId);

        if (!planIncludesCategory)
            return new AccessCheckResult
            {
                HasAccess = false,
                ShowPaywall = true,
                DenialReason = "Your current plan does not include this category. Please upgrade."
            };

        return new AccessCheckResult { HasAccess = true };
    }

    private async Task<List<Guid>> GetAccessibleCategoryIdsAsync(Guid userId)
    {
        var activeSubscription = await _context.Subscriptions
            .Include(s => s.Plan)
                .ThenInclude(p => p.IncludedCategories)
            .Where(s => s.UserId == userId &&
                (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync();

        if (activeSubscription == null)
            return new List<Guid>();

        return activeSubscription.Plan.IncludedCategories.Select(c => c.Id).ToList();
    }

    private async Task<Tip?> GetTipOfTheDayInternalAsync()
    {
        return await _context.Tips
            .Include(t => t.Category)
            .Where(t => t.Status == TipStatus.Published)
            .OrderByDescending(t => t.PublishedAt)
            .FirstOrDefaultAsync();
    }
}
