using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Payments.Services;
using AndyTipster.Application.Plans.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Orchestrates checkout flow: promo code validation, trial handling, payment initiation.
/// </summary>
public class CheckoutService : ICheckoutService
{
    private readonly AndyTipsterDbContext _db;
    private readonly IPayPalService _paypal;
    private readonly IStripeService _stripe;
    private readonly IPromoCodeService _promoService;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        AndyTipsterDbContext db,
        IPayPalService paypal,
        IStripeService stripe,
        IPromoCodeService promoService,
        ILogger<CheckoutService> logger)
    {
        _db = db;
        _paypal = paypal;
        _stripe = stripe;
        _promoService = promoService;
        _logger = logger;
    }

    public async Task<CheckoutSessionDto> InitiateCheckoutAsync(Guid userId, CreateSubscriptionRequest request, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([request.PlanId], ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        // Validate promo code if provided
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var validation = await _promoService.ValidatePromoCodeAsync(request.PromoCode, request.PlanId, ct);
            if (!validation.IsValid)
                throw new InvalidOperationException(validation.ErrorMessage ?? "Invalid promo code.");
        }

        var sessionId = Guid.NewGuid().ToString("N");

        if (request.Provider == PaymentProvider.PayPal)
        {
            var result = await _paypal.CreateSubscriptionAsync(
                userId, request.PlanId,
                request.ReturnUrl ?? "/subscriber/billing",
                request.CancelUrl ?? "/checkout",
                ct);

            if (!result.Success)
                throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create PayPal subscription.");

            return new CheckoutSessionDto(sessionId, result.ApprovalUrl, null, "PayPal", true);
        }
        else // Stripe
        {
            var result = await _stripe.CreateSubscriptionAsync(userId, request.PlanId, null, ct);

            if (!result.Success)
                throw new InvalidOperationException(result.ErrorMessage ?? "Failed to create Stripe subscription.");

            return new CheckoutSessionDto(sessionId, null, null, "Stripe", false);
        }
    }

    public async Task<CheckoutConfirmationDto> ConfirmCheckoutAsync(Guid userId, string sessionId, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (subscription is null)
            return new CheckoutConfirmationDto(false, null, null, null, null, "No subscription found.");

        return new CheckoutConfirmationDto(
            true,
            subscription.Id,
            subscription.Plan.Name,
            subscription.CurrentPeriodEnd,
            subscription.Plan.Price,
            null
        );
    }

    public async Task<CheckoutSummaryDto> GetCheckoutSummaryAsync(Guid planId, string? promoCode, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([planId], ct)
            ?? throw new KeyNotFoundException("Plan not found.");

        decimal finalPrice = plan.Price;
        decimal? discountAmount = null;
        string? promoApplied = null;

        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var validation = await _promoService.ValidatePromoCodeAsync(promoCode, planId, ct);
            if (validation.IsValid)
            {
                finalPrice = validation.DiscountedPrice ?? plan.Price;
                discountAmount = validation.DiscountAmount;
                promoApplied = promoCode.ToUpperInvariant();
            }
        }

        DateTime? trialEnd = plan.TrialPeriodDays > 0 ? DateTime.UtcNow.AddDays(plan.TrialPeriodDays) : null;
        DateTime? firstBilling = trialEnd ?? DateTime.UtcNow;

        return new CheckoutSummaryDto(
            plan.Id, plan.Name, plan.Price, finalPrice,
            discountAmount, promoApplied,
            plan.TrialPeriodDays, trialEnd, firstBilling,
            plan.Currency.ToString(), plan.BillingCycle.ToString()
        );
    }
}
