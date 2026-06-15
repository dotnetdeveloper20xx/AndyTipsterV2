using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Application.Payments.DTOs;

public record CreateSubscriptionRequest(
    Guid PlanId,
    PaymentProvider Provider,
    string? PromoCode,
    string? ReturnUrl,
    string? CancelUrl
);

public record SubscriptionResult(
    bool Success,
    string? SubscriptionId,
    string? ApprovalUrl,
    string? ErrorMessage,
    SubscriptionStatus? Status
);

public record PaymentRecordDto(
    Guid Id,
    Guid SubscriptionId,
    decimal Amount,
    decimal Fees,
    decimal Net,
    Currency Currency,
    PaymentProvider Provider,
    string ExternalTransactionId,
    string Status,
    DateTime PaidAt
);

public record WebhookProcessingResult(
    bool Success,
    string? EventId,
    string? EventType,
    string? ErrorMessage,
    bool IsDuplicate
);

public record TransactionSearchDto(
    string? SearchTerm,
    DateTime? StartDate,
    DateTime? EndDate,
    string? Status,
    PaymentProvider? Provider,
    int Page = 1,
    int PageSize = 25,
    string SortBy = "PaidAt",
    string SortDirection = "desc"
);

public record TransactionListResult(
    List<PaymentRecordDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public record RevenueAnalyticsDto(
    decimal Mrr,
    decimal Arr,
    decimal ChurnRate,
    Dictionary<string, decimal> RevenueByPlan,
    int ActiveSubscribers,
    int NewSubscribersThisMonth,
    List<RevenueTrendPoint> RevenueTrend,
    List<SubscriberTrendPoint> SubscriberTrend
);

public record RevenueTrendPoint(DateTime Date, decimal Revenue);
public record SubscriberTrendPoint(DateTime Date, int Count);

public record RefundRequest(
    string ExternalTransactionId,
    decimal? Amount,
    string? Reason
);

public record RefundResult(
    bool Success,
    string? RefundId,
    string? ErrorMessage
);
