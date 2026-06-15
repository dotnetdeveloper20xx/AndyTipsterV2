using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Subscriptions.DTOs;

namespace AndyTipster.Application.Subscriptions.Services;

/// <summary>
/// Manages subscription lifecycle: retrieval, upgrades, downgrades, cancellation, and admin analytics.
/// </summary>
public interface ISubscriptionService
{
    Task<SubscriptionDto?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken ct = default);
    Task<SubscriptionDto?> GetActiveSubscriptionForUserAsync(Guid userId, CancellationToken ct = default);
    Task<SubscriptionSelfServiceDto?> GetSelfServiceInfoAsync(Guid userId, CancellationToken ct = default);
    Task<UpgradeDowngradeResult> UpgradePlanAsync(Guid userId, UpgradeDowngradeRequest request, CancellationToken ct = default);
    Task<bool> CancelSubscriptionAsync(Guid userId, CancelSubscriptionRequest request, CancellationToken ct = default);
    Task<TransactionListResult> GetTransactionsAsync(TransactionSearchDto search, CancellationToken ct = default);
    Task<RevenueAnalyticsDto> GetRevenueAnalyticsAsync(CancellationToken ct = default);
    Task<AdminDashboardSummaryDto> GetAdminDashboardSummaryAsync(CancellationToken ct = default);
}
