using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Subscriptions.DTOs;
using AndyTipster.Application.Subscriptions.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AndyTipsterDbContext _db;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(AndyTipsterDbContext db, ILogger<SubscriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, ct);
        return sub is null ? null : MapToDto(sub);
    }

    public async Task<SubscriptionDto?> GetActiveSubscriptionForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);
        return sub is null ? null : MapToDto(sub);
    }

    public async Task<SubscriptionSelfServiceDto?> GetSelfServiceInfoAsync(Guid userId, CancellationToken ct = default)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Payments.OrderByDescending(p => p.PaidAt).Take(20))
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (sub is null) return null;

        return new SubscriptionSelfServiceDto(
            sub.Id, sub.Plan.Name, sub.Plan.Price,
            sub.Plan.Currency.ToString(), sub.Plan.BillingCycle.ToString(),
            sub.Status, sub.CurrentPeriodEnd,
            sub.Provider.ToString(), null,
            sub.Payments.Select(p => new PaymentHistoryItemDto(
                p.Id, p.Amount, p.Currency.ToString(), p.Status, p.PaidAt, p.Provider.ToString()
            )).ToList()
        );
    }

    public async Task<UpgradeDowngradeResult> UpgradePlanAsync(Guid userId, UpgradeDowngradeRequest request, CancellationToken ct = default)
    {
        var currentSub = await _db.Subscriptions
            .Include(s => s.Plan).ThenInclude(p => p.UpgradePaths)
            .Where(s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync(ct);

        if (currentSub is null)
            return new UpgradeDowngradeResult(false, null, null, "No active subscription found.");

        // Check if target plan is in allowed paths
        var allPaths = await _db.PlanTransitionPaths
            .Where(p => p.SourcePlanId == currentSub.PlanId)
            .ToListAsync(ct);

        if (!allPaths.Any(p => p.TargetPlanId == request.TargetPlanId))
            return new UpgradeDowngradeResult(false, null, null, "This plan change is not allowed.");

        var targetPlan = await _db.Plans.FindAsync([request.TargetPlanId], ct);
        if (targetPlan is null)
            return new UpgradeDowngradeResult(false, null, null, "Target plan not found.");

        // Calculate proration
        var daysRemaining = (currentSub.CurrentPeriodEnd - DateTime.UtcNow)?.TotalDays ?? 0;
        var totalDays = (currentSub.CurrentPeriodEnd - currentSub.StartDate)?.TotalDays ?? 30;
        var creditAmount = currentSub.Plan.Price * (decimal)(daysRemaining / totalDays);
        var proratedAmount = Math.Max(0, targetPlan.Price - creditAmount);

        // Cancel old subscription
        currentSub.Status = SubscriptionStatus.Cancelled;
        currentSub.CancelledAt = DateTime.UtcNow;

        // Create new subscription
        var newSub = new Domain.Entities.Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = request.TargetPlanId,
            Provider = currentSub.Provider,
            ExternalSubscriptionId = $"UPGRADE-{Guid.NewGuid():N}"[..24],
            Status = SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            CurrentPeriodEnd = CalculatePeriodEnd(targetPlan.BillingCycle),
            CreatedAt = DateTime.UtcNow
        };
        _db.Subscriptions.Add(newSub);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} upgraded from plan {OldPlan} to {NewPlan}", userId, currentSub.PlanId, request.TargetPlanId);
        return new UpgradeDowngradeResult(true, newSub.Id, proratedAmount, null);
    }

    public async Task<bool> CancelSubscriptionAsync(Guid userId, CancelSubscriptionRequest request, CancellationToken ct = default)
    {
        var sub = await _db.Subscriptions
            .Where(s => s.UserId == userId && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .FirstOrDefaultAsync(ct);

        if (sub is null) return false;

        sub.Status = SubscriptionStatus.Cancelled;
        sub.CancelledAt = DateTime.UtcNow;
        // Access maintained until period end
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} cancelled subscription {SubId}. Access until {PeriodEnd}",
            userId, sub.Id, sub.CurrentPeriodEnd);
        return true;
    }

    public async Task<TransactionListResult> GetTransactionsAsync(TransactionSearchDto search, CancellationToken ct = default)
    {
        var query = _db.Payments.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.SearchTerm))
            query = query.Where(p => p.ExternalTransactionId.Contains(search.SearchTerm));

        if (search.StartDate.HasValue)
            query = query.Where(p => p.PaidAt >= search.StartDate.Value);
        if (search.EndDate.HasValue)
            query = query.Where(p => p.PaidAt <= search.EndDate.Value);
        if (!string.IsNullOrWhiteSpace(search.Status))
            query = query.Where(p => p.Status == search.Status);
        if (search.Provider.HasValue)
            query = query.Where(p => p.Provider == search.Provider.Value);

        var totalCount = await query.CountAsync(ct);

        query = search.SortDirection == "asc"
            ? query.OrderBy(p => p.PaidAt)
            : query.OrderByDescending(p => p.PaidAt);

        var items = await query
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .Select(p => new PaymentRecordDto(
                p.Id, p.SubscriptionId, p.Amount, p.Fees, p.Net,
                p.Currency, p.Provider, p.ExternalTransactionId, p.Status, p.PaidAt
            ))
            .ToListAsync(ct);

        return new TransactionListResult(items, totalCount, search.Page, search.PageSize);
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(CancellationToken ct = default)
    {
        var activeSubscriptions = await _db.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing)
            .ToListAsync(ct);

        var mrr = activeSubscriptions.Sum(s => GetMonthlyEquivalent(s.Plan.Price, s.Plan.BillingCycle));
        var arr = mrr * 12;

        var totalEver = await _db.Subscriptions.CountAsync(ct);
        var cancelled = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Cancelled, ct);
        var churnRate = totalEver > 0 ? (decimal)cancelled / totalEver * 100 : 0;

        var revenueByPlan = activeSubscriptions
            .GroupBy(s => s.Plan.Name)
            .ToDictionary(g => g.Key, g => g.Sum(s => GetMonthlyEquivalent(s.Plan.Price, s.Plan.BillingCycle)));

        // Revenue trend (last 12 months)
        var startDate = DateTime.UtcNow.AddMonths(-12);
        var payments = await _db.Payments
            .Where(p => p.PaidAt >= startDate && p.Status == "completed" || p.Status == "succeeded")
            .ToListAsync(ct);

        var revenueTrend = payments
            .GroupBy(p => new DateTime(p.PaidAt.Year, p.PaidAt.Month, 1))
            .Select(g => new RevenueTrendPoint(g.Key, g.Sum(p => p.Amount)))
            .OrderBy(r => r.Date)
            .ToList();

        // Subscriber trend
        var subs = await _db.Subscriptions
            .Where(s => s.CreatedAt >= startDate)
            .ToListAsync(ct);

        var subscriberTrend = subs
            .GroupBy(s => new DateTime(s.CreatedAt.Year, s.CreatedAt.Month, 1))
            .Select(g => new SubscriberTrendPoint(g.Key, g.Count()))
            .OrderBy(s => s.Date)
            .ToList();

        return new RevenueAnalyticsDto(
            mrr, arr, churnRate, revenueByPlan,
            activeSubscriptions.Count,
            subs.Count(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
            revenueTrend, subscriberTrend
        );
    }

    public async Task<AdminDashboardSummaryDto> GetAdminDashboardSummaryAsync(CancellationToken ct = default)
    {
        var totalSubscribers = await _db.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing, ct);

        var activeSubscriptions = await _db.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing)
            .ToListAsync(ct);

        var mrr = activeSubscriptions.Sum(s => GetMonthlyEquivalent(s.Plan.Price, s.Plan.BillingCycle));

        var tipsToday = await _db.Tips
            .CountAsync(t => t.CreatedAt.Date == DateTime.UtcNow.Date, ct);

        var recentSignups = await _db.Subscriptions
            .CountAsync(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-7), ct);

        var paymentAlerts = await _db.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.PastDue, ct);

        // Revenue trend (last 6 months)
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        var payments = await _db.Payments
            .Where(p => p.PaidAt >= sixMonthsAgo)
            .ToListAsync(ct);

        var revenueTrend = payments
            .GroupBy(p => new DateTime(p.PaidAt.Year, p.PaidAt.Month, 1))
            .Select(g => new RevenueTrendPointDto(g.Key, g.Sum(p => p.Amount)))
            .OrderBy(r => r.Date)
            .ToList();

        var subscriberTrend = (await _db.Subscriptions
            .Where(s => s.CreatedAt >= sixMonthsAgo)
            .ToListAsync(ct))
            .GroupBy(s => new DateTime(s.CreatedAt.Year, s.CreatedAt.Month, 1))
            .Select(g => new SubscriberTrendPointDto(g.Key, g.Count()))
            .OrderBy(s => s.Date)
            .ToList();

        // Recent activity from audit log
        var recentActivity = await _db.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new RecentActivityDto(a.ActionType, a.TargetEntity, null, a.Timestamp))
            .ToListAsync(ct);

        return new AdminDashboardSummaryDto(
            totalSubscribers, mrr, tipsToday, recentSignups, paymentAlerts,
            revenueTrend, subscriberTrend, recentActivity
        );
    }

    private static decimal GetMonthlyEquivalent(decimal price, BillingCycle cycle) => cycle switch
    {
        BillingCycle.Weekly => price * 4.33m,
        BillingCycle.Monthly => price,
        BillingCycle.Quarterly => price / 3,
        BillingCycle.SemiAnnual => price / 6,
        BillingCycle.Annual => price / 12,
        _ => price
    };

    private static DateTime CalculatePeriodEnd(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Weekly => DateTime.UtcNow.AddDays(7),
        BillingCycle.Monthly => DateTime.UtcNow.AddMonths(1),
        BillingCycle.Quarterly => DateTime.UtcNow.AddMonths(3),
        BillingCycle.SemiAnnual => DateTime.UtcNow.AddMonths(6),
        BillingCycle.Annual => DateTime.UtcNow.AddYears(1),
        _ => DateTime.UtcNow.AddMonths(1)
    };

    private static SubscriptionDto MapToDto(Domain.Entities.Subscription sub) => new(
        sub.Id, sub.UserId, sub.PlanId, sub.Plan.Name,
        sub.Provider, sub.ExternalSubscriptionId, sub.Status,
        sub.StartDate, sub.CurrentPeriodEnd, sub.TrialEndDate,
        sub.CancelledAt, sub.GracePeriodEndsAt, sub.CreatedAt
    );
}
