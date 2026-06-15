using AndyTipster.Application.Payments.DTOs;

namespace AndyTipster.Application.Payments.Services;

/// <summary>
/// Stripe subscription and payment operations.
/// Stub implementation — actual HTTP calls to Stripe API will be added when API keys are configured.
/// </summary>
public interface IStripeService
{
    Task<SubscriptionResult> CreateSubscriptionAsync(Guid userId, Guid planId, string? paymentMethodId, CancellationToken ct = default);
    Task<SubscriptionResult> ActivateSubscriptionAsync(string subscriptionId, CancellationToken ct = default);
    Task<bool> CancelSubscriptionAsync(string subscriptionId, string reason, CancellationToken ct = default);
    Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default);
}
