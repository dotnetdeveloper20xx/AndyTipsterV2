using AndyTipster.Application.Payments.DTOs;

namespace AndyTipster.Application.Payments.Services;

/// <summary>
/// Orchestrates the checkout flow: promo code validation, trial handling, and payment initiation.
/// </summary>
public interface ICheckoutService
{
    Task<CheckoutSessionDto> InitiateCheckoutAsync(Guid userId, CreateSubscriptionRequest request, CancellationToken ct = default);
    Task<CheckoutConfirmationDto> ConfirmCheckoutAsync(Guid userId, string sessionId, CancellationToken ct = default);
    Task<CheckoutSummaryDto> GetCheckoutSummaryAsync(Guid planId, string? promoCode, CancellationToken ct = default);
}

public record CheckoutSessionDto(
    string SessionId,
    string? ApprovalUrl,
    string? ClientSecret,
    string PaymentProvider,
    bool RequiresRedirect
);

public record CheckoutConfirmationDto(
    bool Success,
    Guid? SubscriptionId,
    string? PlanName,
    DateTime? NextBillingDate,
    decimal? AmountCharged,
    string? ErrorMessage
);

public record CheckoutSummaryDto(
    Guid PlanId,
    string PlanName,
    decimal OriginalPrice,
    decimal FinalPrice,
    decimal? DiscountAmount,
    string? PromoCodeApplied,
    int TrialDays,
    DateTime? TrialEndDate,
    DateTime? FirstBillingDate,
    string Currency,
    string BillingCycle
);
