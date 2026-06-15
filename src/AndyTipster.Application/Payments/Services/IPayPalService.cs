using AndyTipster.Application.Payments.DTOs;

namespace AndyTipster.Application.Payments.Services;

/// <summary>
/// PayPal subscription and payment operations.
/// Stub implementation — actual HTTP calls to PayPal API will be added when API keys are configured.
/// </summary>
public interface IPayPalService
{
    Task<SubscriptionResult> CreateSubscriptionAsync(Guid userId, Guid planId, string returnUrl, string cancelUrl, CancellationToken ct = default);
    Task<SubscriptionResult> ActivateSubscriptionAsync(string subscriptionId, CancellationToken ct = default);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, string reason, CancellationToken ct = default);
    Task<bool> PauseSubscriptionAsync(string subscriptionId, string reason, CancellationToken ct = default);
    Task<bool> ResumeSubscriptionAsync(string subscriptionId, CancellationToken ct = default);
    Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default);
    Task<RefundResult> ProcessRefundAsync(string transactionId, decimal? amount, string? reason, CancellationToken ct = default);
    Task<string?> SyncPlanAsync(Guid planId, CancellationToken ct = default);
}
