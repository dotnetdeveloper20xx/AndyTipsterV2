using AndyTipster.Application.Analytics.DTOs;
using AndyTipster.Application.Analytics.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AndyTipster.Infrastructure.Services.Analytics;

public class PublicAnalyticsService : IPublicAnalyticsService
{
    private readonly AndyTipsterDbContext _db;

    public PublicAnalyticsService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<PublicStatsDto> GetPublicStatsAsync()
    {
        var tips = await _db.Tips
            .Where(t => t.Status == TipStatus.Published || t.Status == TipStatus.Archived)
            .Where(t => t.Result != null)
            .ToListAsync();

        var totalTips = tips.Count;
        var won = tips.Count(t => t.Result == TipResult.Won);
        var lost = tips.Count(t => t.Result == TipResult.Lost);
        var voidCount = tips.Count(t => t.Result == TipResult.Void);
        var push = tips.Count(t => t.Result == TipResult.Push);

        var totalStaked = tips.Sum(t => (decimal)t.Stake);
        var totalProfitLoss = tips.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake));
        var strikeRate = totalTips > 0 ? (decimal)won / totalTips * 100 : 0;
        var roi = totalStaked > 0 ? totalProfitLoss / totalStaked * 100 : 0;

        var profitOverTime = tips
            .OrderBy(t => t.EventDate)
            .GroupBy(t => t.EventDate.Date)
            .Select(g => new { Date = g.Key, DailyProfit = g.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake)) })
            .ToList();

        var cumulativeProfit = 0m;
        var profitOverTimeList = profitOverTime.Select(p =>
        {
            cumulativeProfit += p.DailyProfit;
            return new ProfitOverTimeDto
            {
                Date = p.Date,
                DailyProfit = p.DailyProfit,
                CumulativeProfit = cumulativeProfit
            };
        }).ToList();

        var winRateTrends = tips
            .GroupBy(t => $"{t.EventDate.Year}-{t.EventDate.Month:D2}")
            .OrderBy(g => g.Key)
            .Select(g => new WinRateTrendDto
            {
                Period = g.Key,
                WinRate = g.Count() > 0 ? (decimal)g.Count(t => t.Result == TipResult.Won) / g.Count() * 100 : 0,
                TipCount = g.Count()
            })
            .ToList();

        var categoryComparisons = await _db.Tips
            .Where(t => (t.Status == TipStatus.Published || t.Status == TipStatus.Archived) && t.Result != null)
            .GroupBy(t => new { t.CategoryId, t.Category!.Name })
            .Select(g => new CategoryComparisonDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                ProfitLoss = g.Sum(t => t.Result == TipResult.Won ? (t.Odds * t.Stake) - t.Stake :
                                       t.Result == TipResult.Lost ? -t.Stake : 0),
                StrikeRate = g.Count() > 0 ? (decimal)g.Count(t => t.Result == TipResult.Won) / g.Count() * 100 : 0,
                ROI = g.Sum(t => (decimal)t.Stake) > 0
                    ? g.Sum(t => t.Result == TipResult.Won ? (t.Odds * t.Stake) - t.Stake :
                                 t.Result == TipResult.Lost ? -t.Stake : 0) / g.Sum(t => (decimal)t.Stake) * 100
                    : 0,
                TipCount = g.Count()
            })
            .ToListAsync();

        var monthlyPnL = tips
            .GroupBy(t => new { t.EventDate.Year, t.EventDate.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Select(g => new MonthlyPnLDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                ProfitLoss = g.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake)),
                TipCount = g.Count(),
                StrikeRate = g.Count() > 0 ? (decimal)g.Count(t => t.Result == TipResult.Won) / g.Count() * 100 : 0
            })
            .ToList();

        return new PublicStatsDto
        {
            TotalProfitLoss = totalProfitLoss,
            StrikeRate = strikeRate,
            ROI = roi,
            TotalTips = totalTips,
            Won = won,
            Lost = lost,
            Void = voidCount,
            Push = push,
            Last30Days = await GetLast30DaysSummaryAsync(),
            ProfitOverTime = profitOverTimeList,
            WinRateTrends = winRateTrends,
            CategoryComparisons = categoryComparisons,
            MonthlyPnL = monthlyPnL
        };
    }

    public async Task<Last30DaysSummaryDto> GetLast30DaysSummaryAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var tips = await _db.Tips
            .Where(t => (t.Status == TipStatus.Published || t.Status == TipStatus.Archived) && t.Result != null)
            .Where(t => t.EventDate >= thirtyDaysAgo)
            .ToListAsync();

        var totalTips = tips.Count;
        var won = tips.Count(t => t.Result == TipResult.Won);
        var lost = tips.Count(t => t.Result == TipResult.Lost);
        var totalStaked = tips.Sum(t => (decimal)t.Stake);
        var profitLoss = tips.Sum(t => CalculatePnL(t.Result!.Value, t.Odds, t.Stake));
        var strikeRate = totalTips > 0 ? (decimal)won / totalTips * 100 : 0;
        var roi = totalStaked > 0 ? profitLoss / totalStaked * 100 : 0;

        return new Last30DaysSummaryDto
        {
            ProfitLoss = profitLoss,
            StrikeRate = strikeRate,
            ROI = roi,
            TotalTips = totalTips,
            Won = won,
            Lost = lost
        };
    }

    public async Task<byte[]> ExportResultsCsvAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId)
    {
        var query = _db.Tips
            .Include(t => t.Category)
            .Where(t => (t.Status == TipStatus.Published || t.Status == TipStatus.Archived) && t.Result != null);

        if (startDate.HasValue) query = query.Where(t => t.EventDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(t => t.EventDate <= endDate.Value);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);

        var tips = await query.OrderByDescending(t => t.EventDate).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Date,Race,Selection,Odds,Stake,Category,Result,P&L");
        foreach (var tip in tips)
        {
            var pnl = CalculatePnL(tip.Result!.Value, tip.Odds, tip.Stake);
            sb.AppendLine($"{tip.EventDate:yyyy-MM-dd},{EscapeCsv(tip.RaceName)},{EscapeCsv(tip.Selection)},{tip.Odds},{tip.Stake},{EscapeCsv(tip.Category?.Name ?? "")},{tip.Result},{pnl:F2}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> ExportResultsPdfAsync(DateTime? startDate, DateTime? endDate, Guid? categoryId)
    {
        // PDF generation stub - returns a simple text-based representation
        // In production, use a library like QuestPDF or iTextSharp
        var csvData = await ExportResultsCsvAsync(startDate, endDate, categoryId);
        var pdfContent = $"AndyTipster Results Export\nGenerated: {DateTime.UtcNow:yyyy-MM-dd HH:mm}\n\n{Encoding.UTF8.GetString(csvData)}";
        return Encoding.UTF8.GetBytes(pdfContent);
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

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
