using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Application.Subscriptions.DTOs;

public record SubscriptionDto(
    Guid Id,
    Guid UserId,
    Guid PlanId,
    string PlanName,
    PaymentProvider Provider,
    string ExternalSubscriptionId,
    SubscriptionStatus Status,
    DateTime StartDate,
    DateTime? CurrentPeriodEnd,
    DateTime? TrialEndDate,
    DateTime? CancelledAt,
    DateTime? GracePeriodEndsAt,
    DateTime CreatedAt
);

public record SubscriptionSelfServiceDto(
    Guid SubscriptionId,
    string PlanName,
    decimal Price,
    string Currency,
    string BillingCycle,
    SubscriptionStatus Status,
    DateTime? NextBillingDate,
    string PaymentProvider,
    string? PaymentMethodLast4,
    List<PaymentHistoryItemDto> PaymentHistory
);

public record PaymentHistoryItemDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTime PaidAt,
    string Provider
);

public record UpgradeDowngradeRequest(
    Guid TargetPlanId
);

public record UpgradeDowngradeResult(
    bool Success,
    Guid? NewSubscriptionId,
    decimal? ProratedAmount,
    string? ErrorMessage
);

public record CancelSubscriptionRequest(
    string? Reason
);

public record AdminDashboardSummaryDto(
    int TotalSubscribers,
    decimal Mrr,
    int TipsPublishedToday,
    int RecentSignups,
    int PaymentAlerts,
    List<RevenueTrendPointDto> RevenueTrend,
    List<SubscriberTrendPointDto> SubscriberTrend,
    List<RecentActivityDto> RecentActivity
);

public record RevenueTrendPointDto(DateTime Date, decimal Revenue);
public record SubscriberTrendPointDto(DateTime Date, int Count);

public record RecentActivityDto(
    string Action,
    string Description,
    string? UserName,
    DateTime Timestamp
);
