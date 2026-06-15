namespace AndyTipster.Application.Analytics.DTOs;

// === Public Performance Analytics (Task 18.1) ===

public record PublicStatsDto
{
    public decimal TotalProfitLoss { get; init; }
    public decimal StrikeRate { get; init; }
    public decimal ROI { get; init; }
    public int TotalTips { get; init; }
    public int Won { get; init; }
    public int Lost { get; init; }
    public int Void { get; init; }
    public int Push { get; init; }
    public Last30DaysSummaryDto Last30Days { get; init; } = new();
    public List<ProfitOverTimeDto> ProfitOverTime { get; init; } = new();
    public List<WinRateTrendDto> WinRateTrends { get; init; } = new();
    public List<CategoryComparisonDto> CategoryComparisons { get; init; } = new();
    public List<MonthlyPnLDto> MonthlyPnL { get; init; } = new();
}

public record Last30DaysSummaryDto
{
    public decimal ProfitLoss { get; init; }
    public decimal StrikeRate { get; init; }
    public decimal ROI { get; init; }
    public int TotalTips { get; init; }
    public int Won { get; init; }
    public int Lost { get; init; }
}

public record ProfitOverTimeDto
{
    public DateTime Date { get; init; }
    public decimal CumulativeProfit { get; init; }
    public decimal DailyProfit { get; init; }
}

public record WinRateTrendDto
{
    public string Period { get; init; } = string.Empty;
    public decimal WinRate { get; init; }
    public int TipCount { get; init; }
}

public record CategoryComparisonDto
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal ProfitLoss { get; init; }
    public decimal StrikeRate { get; init; }
    public decimal ROI { get; init; }
    public int TipCount { get; init; }
}

public record MonthlyPnLDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal ProfitLoss { get; init; }
    public int TipCount { get; init; }
    public decimal StrikeRate { get; init; }
}

// === Subscriber Performance Dashboard (Task 18.2) ===

public record SubscriberPerformanceDto
{
    public decimal TotalProfitLoss { get; init; }
    public decimal StrikeRate { get; init; }
    public int TotalTips { get; init; }
    public int Won { get; init; }
    public int Lost { get; init; }
    public int CurrentStreak { get; init; }
    public string StreakType { get; init; } = string.Empty; // "winning" or "losing"
    public int LongestWinStreak { get; init; }
    public int LongestLoseStreak { get; init; }
    public List<CategoryComparisonDto> CategoryBreakdown { get; init; } = new();
    public List<MonthlyPerformanceSummaryDto> MonthlySummaries { get; init; } = new();
}

public record SubscriberPerformanceFilterDto
{
    public Guid? CategoryId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public record MonthlyPerformanceSummaryDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal ProfitLoss { get; init; }
    public int TipCount { get; init; }
    public int Won { get; init; }
    public int Lost { get; init; }
    public decimal StrikeRate { get; init; }
    public decimal ROI { get; init; }
}

// === Admin Revenue Analytics (Task 18.3) ===

public record RevenueAnalyticsDto
{
    public decimal MRR { get; init; }
    public decimal ARR { get; init; }
    public decimal ChurnRate { get; init; }
    public int TotalActiveSubscribers { get; init; }
    public int NewSubscribersThisMonth { get; init; }
    public int CancelledThisMonth { get; init; }
    public decimal AverageLTV { get; init; }
    public List<RevenueByPlanDto> RevenueByPlan { get; init; } = new();
    public List<RevenueTrendDto> RevenueTrends { get; init; } = new();
    public List<SubscriberGrowthDto> SubscriberGrowth { get; init; } = new();
    public SubscriberForecastDto Forecast { get; init; } = new();
}

public record RevenueByPlanDto
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int SubscriberCount { get; init; }
    public decimal AverageLTV { get; init; }
}

public record RevenueTrendDto
{
    public DateTime Date { get; init; }
    public decimal Revenue { get; init; }
    public string Provider { get; init; } = string.Empty; // "PayPal", "Stripe", "Combined"
    public string Granularity { get; init; } = string.Empty; // "daily", "weekly", "monthly"
}

public record SubscriberGrowthDto
{
    public DateTime Date { get; init; }
    public int TotalSubscribers { get; init; }
    public int NewSubscribers { get; init; }
    public int Churned { get; init; }
    public int NetGrowth { get; init; }
}

public record SubscriberForecastDto
{
    public int ProjectedSubscribers30Days { get; init; }
    public int ProjectedSubscribers90Days { get; init; }
    public decimal ProjectedMRR30Days { get; init; }
    public decimal ProjectedMRR90Days { get; init; }
}

public record RevenueFilterDto
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Granularity { get; init; } = "monthly"; // daily, weekly, monthly
    public string? Provider { get; init; } // PayPal, Stripe, or null for combined
}
