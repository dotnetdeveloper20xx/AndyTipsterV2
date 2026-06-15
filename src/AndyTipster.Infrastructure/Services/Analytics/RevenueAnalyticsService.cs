using AndyTipster.Application.Analytics.DTOs;
using AndyTipster.Application.Analytics.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.Analytics;

public class RevenueAnalyticsService : IRevenueAnalyticsService
{
    private readonly AndyTipsterDbContext _db;

    public RevenueAnalyticsService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(RevenueFilterDto? filter)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Active subscriptions
        var activeSubscriptions = await _db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Include(s => s.Plan)
            .ToListAsync();

        var totalActive = activeSubscriptions.Count;

        // MRR calculation: sum of monthly-equivalent prices for all active subscribers
        var mrr = activeSubscriptions.Sum(s => CalculateMonthlyEquivalent(s.Plan?.Price ?? 0, s.Plan?.BillingCycle ?? BillingCycle.Monthly));
        var arr = mrr * 12;

        // New subscribers this month
        var newThisMonth = await _db.Subscriptions
            .CountAsync(s => s.CreatedAt >= startOfMonth);

        // Cancelled this month
        var cancelledThisMonth = await _db.Subscriptions
            .CountAsync(s => s.Status == SubscriptionStatus.Cancelled && s.CancelledAt != null && s.CancelledAt >= startOfMonth);

        // Churn rate: cancelled / (active + cancelled) * 100
        var totalForChurn = totalActive + cancelledThisMonth;
        var churnRate = totalForChurn > 0 ? (decimal)cancelledThisMonth / totalForChurn * 100 : 0;

        // Average LTV: average total payments per subscriber
        var payments = await _db.Payments
            .Where(p => p.Status == "Completed")
            .GroupBy(p => p.Subscription.UserId)
            .Select(g => new { UserId = g.Key, Total = g.Sum(p => p.Amount) })
            .ToListAsync();

        var averageLtv = payments.Any() ? payments.Average(p => p.Total) : 0;

        // Revenue by plan
        var revenueByPlan = await _db.Payments
            .Where(p => p.Status == "Completed")
            .GroupBy(p => new { p.Subscription!.PlanId, p.Subscription.Plan!.Name })
            .Select(g => new RevenueByPlanDto
            {
                PlanId = g.Key.PlanId,
                PlanName = g.Key.Name,
                Revenue = g.Sum(p => p.Amount),
                SubscriberCount = g.Select(p => p.Subscription.UserId).Distinct().Count(),
                AverageLTV = g.Select(p => p.Subscription.UserId).Distinct().Count() > 0
                    ? g.Sum(p => p.Amount) / g.Select(p => p.Subscription.UserId).Distinct().Count()
                    : 0
            })
            .ToListAsync();

        // Revenue trends
        var revenueTrends = await GetRevenueTrendsAsync(filter ?? new RevenueFilterDto());

        // Subscriber growth
        var subscriberGrowth = await GetSubscriberGrowthAsync();

        // Forecast
        var forecast = await GetSubscriberForecastAsync();

        return new RevenueAnalyticsDto
        {
            MRR = mrr,
            ARR = arr,
            ChurnRate = churnRate,
            TotalActiveSubscribers = totalActive,
            NewSubscribersThisMonth = newThisMonth,
            CancelledThisMonth = cancelledThisMonth,
            AverageLTV = averageLtv,
            RevenueByPlan = revenueByPlan,
            RevenueTrends = revenueTrends,
            SubscriberGrowth = subscriberGrowth,
            Forecast = forecast
        };
    }

    public async Task<List<RevenueTrendDto>> GetRevenueTrendsAsync(RevenueFilterDto filter)
    {
        var startDate = filter.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = filter.EndDate ?? DateTime.UtcNow;

        var payments = await _db.Payments
            .Where(p => p.Status == "Completed" && p.CreatedAt >= startDate && p.CreatedAt <= endDate)
            .ToListAsync();

        if (!string.IsNullOrEmpty(filter.Provider))
        {
            var providerEnum = Enum.Parse<PaymentProvider>(filter.Provider);
            payments = payments.Where(p => p.Provider == providerEnum).ToList();
        }

        var granularity = filter.Granularity ?? "monthly";
        var trends = new List<RevenueTrendDto>();

        var grouped = granularity switch
        {
            "daily" => payments.GroupBy(p => p.CreatedAt.Date),
            "weekly" => payments.GroupBy(p => p.CreatedAt.Date.AddDays(-(int)p.CreatedAt.DayOfWeek)),
            _ => payments.GroupBy(p => new DateTime(p.CreatedAt.Year, p.CreatedAt.Month, 1))
        };

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            // Combined
            trends.Add(new RevenueTrendDto
            {
                Date = group.Key,
                Revenue = group.Sum(p => p.Amount),
                Provider = "Combined",
                Granularity = granularity
            });

            // PayPal
            var paypalRevenue = group.Where(p => p.Provider == PaymentProvider.PayPal).Sum(p => p.Amount);
            if (paypalRevenue > 0)
            {
                trends.Add(new RevenueTrendDto
                {
                    Date = group.Key,
                    Revenue = paypalRevenue,
                    Provider = "PayPal",
                    Granularity = granularity
                });
            }

            // Stripe
            var stripeRevenue = group.Where(p => p.Provider == PaymentProvider.Stripe).Sum(p => p.Amount);
            if (stripeRevenue > 0)
            {
                trends.Add(new RevenueTrendDto
                {
                    Date = group.Key,
                    Revenue = stripeRevenue,
                    Provider = "Stripe",
                    Granularity = granularity
                });
            }
        }

        return trends;
    }

    public async Task<SubscriberForecastDto> GetSubscriberForecastAsync()
    {
        // Simple linear forecast based on last 3 months growth
        var now = DateTime.UtcNow;
        var threeMonthsAgo = now.AddMonths(-3);

        var monthlyGrowth = await _db.Subscriptions
            .Where(s => s.CreatedAt >= threeMonthsAgo)
            .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
            .Select(g => new { Period = g.Key, Count = g.Count() })
            .ToListAsync();

        var avgMonthlyGrowth = monthlyGrowth.Any() ? monthlyGrowth.Average(m => m.Count) : 0;
        var currentSubscribers = await _db.Subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active);

        var activeSubscriptions = await _db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Include(s => s.Plan)
            .ToListAsync();

        var currentMrr = activeSubscriptions.Sum(s => CalculateMonthlyEquivalent(s.Plan?.Price ?? 0, s.Plan?.BillingCycle ?? BillingCycle.Monthly));
        var avgRevenuePerSubscriber = currentSubscribers > 0 ? currentMrr / currentSubscribers : 0;

        return new SubscriberForecastDto
        {
            ProjectedSubscribers30Days = currentSubscribers + (int)Math.Round(avgMonthlyGrowth),
            ProjectedSubscribers90Days = currentSubscribers + (int)Math.Round(avgMonthlyGrowth * 3),
            ProjectedMRR30Days = currentMrr + (avgRevenuePerSubscriber * (decimal)avgMonthlyGrowth),
            ProjectedMRR90Days = currentMrr + (avgRevenuePerSubscriber * (decimal)(avgMonthlyGrowth * 3))
        };
    }

    private async Task<List<SubscriberGrowthDto>> GetSubscriberGrowthAsync()
    {
        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);

        var created = await _db.Subscriptions
            .Where(s => s.CreatedAt >= sixMonthsAgo)
            .GroupBy(s => s.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var cancelled = await _db.Subscriptions
            .Where(s => s.Status == SubscriptionStatus.Cancelled && s.CancelledAt != null && s.CancelledAt >= sixMonthsAgo)
            .GroupBy(s => s.CancelledAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var allDates = created.Select(c => c.Date)
            .Union(cancelled.Select(c => c.Date))
            .Distinct()
            .OrderBy(d => d);

        var totalRunning = await _db.Subscriptions
            .CountAsync(s => s.CreatedAt < sixMonthsAgo && s.Status == SubscriptionStatus.Active);

        var result = new List<SubscriberGrowthDto>();
        foreach (var date in allDates)
        {
            var newSubs = created.FirstOrDefault(c => c.Date == date)?.Count ?? 0;
            var churned = cancelled.FirstOrDefault(c => c.Date == date)?.Count ?? 0;
            totalRunning += newSubs - churned;

            result.Add(new SubscriberGrowthDto
            {
                Date = date,
                TotalSubscribers = totalRunning,
                NewSubscribers = newSubs,
                Churned = churned,
                NetGrowth = newSubs - churned
            });
        }

        return result;
    }

    private static decimal CalculateMonthlyEquivalent(decimal price, BillingCycle billingCycle)
    {
        return billingCycle switch
        {
            BillingCycle.Weekly => price * 4.33m,
            BillingCycle.Monthly => price,
            BillingCycle.Quarterly => price / 3,
            BillingCycle.SemiAnnual => price / 6,
            BillingCycle.Annual => price / 12,
            _ => price
        };
    }
}
