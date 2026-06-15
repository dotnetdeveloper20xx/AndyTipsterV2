using AndyTipster.Application.Analytics.DTOs;
using AndyTipster.Application.Analytics.Services;
using AndyTipster.Application.Notifications.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.Analytics;

public class SubscriberAnalyticsService : ISubscriberAnalyticsService
{
    private readonly AndyTipsterDbContext _db;
    private readonly INotificationService _notificationService;

    public SubscriberAnalyticsService(AndyTipsterDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<SubscriberPerformanceDto> GetSubscriberPerformanceAsync(Guid userId, SubscriberPerformanceFilterDto? filter)
    {
        // Get the categories the subscriber has access to via their subscriptions
        var subscribedCategoryIds = await _db.Subscriptions
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active)
            .SelectMany(s => s.Plan!.IncludedCategories.Select(c => c.Id))
            .Distinct()
            .ToListAsync();

        var query = _db.Tips
            .Include(t => t.Category)
            .Where(t => (t.Status == TipStatus.Published || t.Status == TipStatus.Archived) && t.Result != null)
            .Where(t => subscribedCategoryIds.Contains(t.CategoryId));

        if (filter?.CategoryId.HasValue == true)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);
        if (filter?.StartDate.HasValue == true)
            query = query.Where(t => t.EventDate >= filter.StartDate.Value);
        if (filter?.EndDate.HasValue == true)
            query = query.Where(t => t.EventDate <= filter.EndDate.Value);

        var tips = await query.OrderBy(t => t.EventDate).ToListAsync();

        var totalTips = tips.Count;
        var won = tips.Count(t => t.Result == TipResult.Won);
        var lost = tips.Count(t => t.Result == TipResult.Lost);
        var totalStaked = tips.Sum(t => (decimal)t.Stake);
        var totalProfitLoss = tips.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake));
        var strikeRate = totalTips > 0 ? (decimal)won / totalTips * 100 : 0;

        // Calculate streaks
        var (currentStreak, streakType, longestWin, longestLose) = CalculateStreaks(tips);

        // Category breakdown
        var categoryBreakdown = tips
            .GroupBy(t => new { t.CategoryId, CategoryName = t.Category?.Name ?? "Unknown" })
            .Select(g => new CategoryComparisonDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProfitLoss = g.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake)),
                StrikeRate = g.Count() > 0 ? (decimal)g.Count(t => t.Result == TipResult.Won) / g.Count() * 100 : 0,
                ROI = g.Sum(t => (decimal)t.Stake) > 0
                    ? g.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake)) / g.Sum(t => (decimal)t.Stake) * 100
                    : 0,
                TipCount = g.Count()
            })
            .ToList();

        // Monthly summaries
        var monthlySummaries = tips
            .GroupBy(t => new { t.EventDate.Year, t.EventDate.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Select(g => new MonthlyPerformanceSummaryDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ProfitLoss = g.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake)),
                TipCount = g.Count(),
                Won = g.Count(t => t.Result == TipResult.Won),
                Lost = g.Count(t => t.Result == TipResult.Lost),
                StrikeRate = g.Count() > 0 ? (decimal)g.Count(t => t.Result == TipResult.Won) / g.Count() * 100 : 0,
                ROI = g.Sum(t => (decimal)t.Stake) > 0
                    ? g.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake)) / g.Sum(t => (decimal)t.Stake) * 100
                    : 0
            })
            .ToList();

        return new SubscriberPerformanceDto
        {
            TotalProfitLoss = totalProfitLoss,
            StrikeRate = strikeRate,
            TotalTips = totalTips,
            Won = won,
            Lost = lost,
            CurrentStreak = currentStreak,
            StreakType = streakType,
            LongestWinStreak = longestWin,
            LongestLoseStreak = longestLose,
            CategoryBreakdown = categoryBreakdown,
            MonthlySummaries = monthlySummaries
        };
    }

    public async Task<List<MonthlyPerformanceSummaryDto>> GetMonthlySummariesAsync(Guid userId, int months = 12)
    {
        var filter = new SubscriberPerformanceFilterDto
        {
            StartDate = DateTime.UtcNow.AddMonths(-months)
        };
        var performance = await GetSubscriberPerformanceAsync(userId, filter);
        return performance.MonthlySummaries;
    }

    public async Task SendMonthlyDigestEmailAsync(Guid userId)
    {
        var summaries = await GetMonthlySummariesAsync(userId, 1);
        var lastMonth = summaries.FirstOrDefault();
        if (lastMonth == null) return;

        var user = await _db.Users.FindAsync(userId);
        if (user == null) return;

        // Use existing notification service to send digest
        await _notificationService.SendNotificationAsync(new Application.Notifications.DTOs.SendNotificationDto
        {
            UserId = userId,
            Type = "MonthlyDigest",
            Title = "Monthly Performance Digest",
            Body = $"Your performance for {lastMonth.Year}-{lastMonth.Month:D2}: " +
                   $"P&L: {lastMonth.ProfitLoss:+0.00;-0.00}, Strike Rate: {lastMonth.StrikeRate:F1}%, " +
                   $"Tips: {lastMonth.TipCount} (Won: {lastMonth.Won}, Lost: {lastMonth.Lost})"
        });
    }

    private static (int currentStreak, string streakType, int longestWin, int longestLose) CalculateStreaks(
        List<Domain.Entities.Tip> tips)
    {
        var orderedTips = tips.Where(t => t.Result == TipResult.Won || t.Result == TipResult.Lost)
            .OrderByDescending(t => t.EventDate)
            .ToList();

        if (!orderedTips.Any())
            return (0, "none", 0, 0);

        // Current streak
        var currentType = orderedTips[0].Result!.Value;
        var currentStreak = 0;
        foreach (var tip in orderedTips)
        {
            if (tip.Result == currentType) currentStreak++;
            else break;
        }

        // Longest streaks
        var longestWin = 0;
        var longestLose = 0;
        var winStreak = 0;
        var loseStreak = 0;

        foreach (var tip in orderedTips.AsEnumerable().Reverse())
        {
            if (tip.Result == TipResult.Won)
            {
                winStreak++;
                loseStreak = 0;
                longestWin = Math.Max(longestWin, winStreak);
            }
            else
            {
                loseStreak++;
                winStreak = 0;
                longestLose = Math.Max(longestLose, loseStreak);
            }
        }

        return (currentStreak, currentType == TipResult.Won ? "winning" : "losing", longestWin, longestLose);
    }

    private static decimal CalculatePnL(TipResult result, decimal odds, int stake)
    {
        return result switch
        {
            TipResult.Won => (odds * stake) - stake,
            TipResult.Lost => -stake,
            TipResult.Void or TipResult.Push => 0,
            _ => 0
        };
    }
}
