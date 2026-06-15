using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Payments.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// Stripe service — stub implementation for API calls, real webhook processing logic.
/// Actual HTTP calls to Stripe will be added when API keys are configured.
/// </summary>
public class StripeService : IStripeService
{
    private readonly AndyTipsterDbContext _db;
    private readonly ILogger<StripeService> _logger;

    public StripeService(AndyTipsterDbContext db, ILogger<StripeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SubscriptionResult> CreateSubscriptionAsync(Guid userId, Guid planId, string? paymentMethodId, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([planId], ct);
        if (plan is null)
            return new SubscriptionResult(false, null, null, "Plan not found.", null);

        // STUB: In production, this calls Stripe Subscriptions API
        _logger.LogInformation("STUB: Creating Stripe subscription for user {UserId}, plan {PlanId}", userId, planId);

        var externalId = $"sub_stub_{Guid.NewGuid():N}"[..24];
        var clientSecret = $"pi_stub_secret_{Guid.NewGuid():N}"[..30];

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            Provider = PaymentProvider.Stripe,
            ExternalSubscriptionId = externalId,
            Status = plan.TrialPeriodDays > 0 ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            CurrentPeriodEnd = CalculatePeriodEnd(plan.BillingCycle),
            TrialEndDate = plan.TrialPeriodDays > 0 ? DateTime.UtcNow.AddDays(plan.TrialPeriodDays) : null,
            CreatedAt = DateTime.UtcNow
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(ct);

        return new SubscriptionResult(true, subscription.Id.ToString(), null, null, subscription.Status);
    }

    public async Task<SubscriptionResult> ActivateSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is null)
            return new SubscriptionResult(false, null, null, "Subscription not found.", null);

        // STUB: In production, verify with Stripe API
        _logger.LogInformation("STUB: Activating Stripe subscription {SubId}", subscriptionId);

        subscription.Status = SubscriptionStatus.Active;
        await _db.SaveChangesAsync(ct);
        return new SubscriptionResult(true, subscription.Id.ToString(), null, null, subscription.Status);
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, string reason, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is null) return false;

        // STUB: In production, call Stripe cancel subscription API
        _logger.LogInformation("STUB: Cancelling Stripe subscription {SubId}. Reason: {Reason}", subscriptionId, reason);

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        // Step 1: Verify signature (stub — validates format)
        if (string.IsNullOrWhiteSpace(signature) || !signature.StartsWith("t="))
        {
            _logger.LogWarning("Stripe webhook signature verification failed");
            return new WebhookProcessingResult(false, null, null, "Invalid webhook signature.", false);
        }

        // Step 2: Parse event
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(payload);
        }
        catch (JsonException)
        {
            return new WebhookProcessingResult(false, null, null, "Invalid JSON payload.", false);
        }

        var root = doc.RootElement;
        var eventId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        var eventType = root.TryGetProperty("type", out var typeProp) ? typeProp.GetString() : null;

        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventType))
            return new WebhookProcessingResult(false, null, null, "Missing event ID or type.", false);

        // Step 3: Idempotency check
        var existingEvent = await _db.WebhookEvents
            .AnyAsync(e => e.ExternalEventId == eventId && e.Provider == PaymentProvider.Stripe, ct);

        if (existingEvent)
        {
            _logger.LogDebug("Duplicate Stripe webhook event {EventId} — skipping", eventId);
            return new WebhookProcessingResult(true, eventId, eventType, null, true);
        }

        // Step 4: Record event
        var webhookEvent = new WebhookEvent
        {
            Id = Guid.NewGuid(),
            ExternalEventId = eventId,
            Provider = PaymentProvider.Stripe,
            EventType = eventType,
            Payload = payload,
            Processed = false,
            ReceivedAt = DateTime.UtcNow
        };
        _db.WebhookEvents.Add(webhookEvent);

        // Step 5: Process by event type
        try
        {
            var data = root.TryGetProperty("data", out var dataProp) ? dataProp : default;
            var obj = data.ValueKind != JsonValueKind.Undefined && data.TryGetProperty("object", out var objProp) ? objProp : default;

            await ProcessStripeEventAsync(eventType, obj, ct);
            webhookEvent.Processed = true;
            webhookEvent.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Processed Stripe webhook {EventType} ({EventId})", eventType, eventId);
            return new WebhookProcessingResult(true, eventId, eventType, null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe webhook {EventType} ({EventId})", eventType, eventId);
            await _db.SaveChangesAsync(ct);
            return new WebhookProcessingResult(false, eventId, eventType, ex.Message, false);
        }
    }

    private async Task ProcessStripeEventAsync(string eventType, JsonElement obj, CancellationToken ct)
    {
        switch (eventType)
        {
            case "invoice.payment_succeeded":
                await HandleInvoicePaymentSucceededAsync(obj, ct);
                break;
            case "invoice.payment_failed":
                await HandleInvoicePaymentFailedAsync(obj, ct);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeletedAsync(obj, ct);
                break;
            default:
                _logger.LogInformation("Unknown Stripe event type: {EventType} — no action taken", eventType);
                break;
        }
    }

    private async Task HandleInvoicePaymentSucceededAsync(JsonElement obj, CancellationToken ct)
    {
        var subscriptionId = obj.TryGetProperty("subscription", out var subProp) ? subProp.GetString() : null;
        if (subscriptionId is null) return;

        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);
        if (subscription is null) return;

        decimal amount = 0;
        var currency = Currency.GBP;
        string paymentIntentId = "";

        if (obj.TryGetProperty("amount_paid", out var amtProp))
            amount = amtProp.GetInt64() / 100m; // Stripe uses smallest currency unit

        if (obj.TryGetProperty("currency", out var currProp))
        {
            var currStr = currProp.GetString()?.ToUpperInvariant();
            if (Enum.TryParse<Currency>(currStr, true, out var parsed))
                currency = parsed;
        }

        if (obj.TryGetProperty("payment_intent", out var piProp))
            paymentIntentId = piProp.GetString() ?? "";

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            Amount = amount,
            Fees = 0, // Stripe fee info requires separate API call
            Net = amount,
            Currency = currency,
            Provider = PaymentProvider.Stripe,
            ExternalTransactionId = paymentIntentId,
            Status = "succeeded",
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);

        // Recover from past-due if applicable
        if (subscription.Status == SubscriptionStatus.PastDue)
        {
            subscription.Status = SubscriptionStatus.Active;
            subscription.GracePeriodEndsAt = null;
        }

        _logger.LogInformation("Stripe payment succeeded for subscription {SubId} — {Amount} {Currency}",
            subscriptionId, amount, currency);
    }

    private async Task HandleInvoicePaymentFailedAsync(JsonElement obj, CancellationToken ct)
    {
        var subscriptionId = obj.TryGetProperty("subscription", out var subProp) ? subProp.GetString() : null;
        if (subscriptionId is null) return;

        var subscription = await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is not null)
        {
            subscription.Status = SubscriptionStatus.PastDue;
            subscription.GracePeriodEndsAt = DateTime.UtcNow.AddDays(subscription.Plan.GracePeriodDays);
            _logger.LogWarning("Stripe payment failed for subscription {SubId} — grace period until {GracePeriodEnd}",
                subscriptionId, subscription.GracePeriodEndsAt);
        }
    }

    private async Task HandleSubscriptionDeletedAsync(JsonElement obj, CancellationToken ct)
    {
        var subscriptionId = obj.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (subscriptionId is null) return;

        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is not null)
        {
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            _logger.LogInformation("Stripe subscription {SubId} deleted/cancelled", subscriptionId);
        }
    }

    private static DateTime CalculatePeriodEnd(BillingCycle cycle) => cycle switch
    {
        BillingCycle.Weekly => DateTime.UtcNow.AddDays(7),
        BillingCycle.Monthly => DateTime.UtcNow.AddMonths(1),
        BillingCycle.Quarterly => DateTime.UtcNow.AddMonths(3),
        BillingCycle.SemiAnnual => DateTime.UtcNow.AddMonths(6),
        BillingCycle.Annual => DateTime.UtcNow.AddYears(1),
        _ => DateTime.UtcNow.AddMonths(1)
    };
}
