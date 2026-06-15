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
/// PayPal service — stub implementation for API calls, real webhook processing logic.
/// Actual HTTP calls to PayPal will be added when API keys are configured.
/// </summary>
public class PayPalService : IPayPalService
{
    private readonly AndyTipsterDbContext _db;
    private readonly ILogger<PayPalService> _logger;
    private const int MaxRetries = 3;

    public PayPalService(AndyTipsterDbContext db, ILogger<PayPalService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<SubscriptionResult> CreateSubscriptionAsync(Guid userId, Guid planId, string returnUrl, string cancelUrl, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([planId], ct);
        if (plan is null)
            return new SubscriptionResult(false, null, null, "Plan not found.", null);

        // STUB: In production, this would call PayPal Subscriptions API
        _logger.LogInformation("STUB: Creating PayPal subscription for user {UserId}, plan {PlanId}", userId, planId);

        var externalId = $"I-STUB{Guid.NewGuid():N}"[..20].ToUpperInvariant();
        var approvalUrl = $"https://www.sandbox.paypal.com/webapps/billing/subscriptions?ba_token={externalId}";

        // Create subscription record
        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanId = planId,
            Provider = PaymentProvider.PayPal,
            ExternalSubscriptionId = externalId,
            Status = plan.TrialPeriodDays > 0 ? SubscriptionStatus.Trialing : SubscriptionStatus.Active,
            StartDate = DateTime.UtcNow,
            CurrentPeriodEnd = CalculatePeriodEnd(plan.BillingCycle),
            TrialEndDate = plan.TrialPeriodDays > 0 ? DateTime.UtcNow.AddDays(plan.TrialPeriodDays) : null,
            CreatedAt = DateTime.UtcNow
        };

        _db.Subscriptions.Add(subscription);
        await _db.SaveChangesAsync(ct);

        return new SubscriptionResult(true, subscription.Id.ToString(), approvalUrl, null, subscription.Status);
    }

    public async Task<SubscriptionResult> ActivateSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is null)
            return new SubscriptionResult(false, null, null, "Subscription not found.", null);

        // STUB: In production, verify with PayPal API
        _logger.LogInformation("STUB: Activating PayPal subscription {SubId}", subscriptionId);

        if (subscription.TrialEndDate.HasValue && subscription.TrialEndDate > DateTime.UtcNow)
            subscription.Status = SubscriptionStatus.Trialing;
        else
            subscription.Status = SubscriptionStatus.Active;

        await _db.SaveChangesAsync(ct);
        return new SubscriptionResult(true, subscription.Id.ToString(), null, null, subscription.Status);
    }

    public async Task<bool> CancelSubscriptionAsync(string subscriptionId, string reason, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is null) return false;

        // STUB: In production, call PayPal cancel subscription API
        _logger.LogInformation("STUB: Cancelling PayPal subscription {SubId}. Reason: {Reason}", subscriptionId, reason);

        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.CancelledAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> PauseSubscriptionAsync(string subscriptionId, string reason, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is null) return false;

        // STUB: In production, call PayPal suspend subscription API
        _logger.LogInformation("STUB: Pausing PayPal subscription {SubId}. Reason: {Reason}", subscriptionId, reason);

        subscription.Status = SubscriptionStatus.Paused;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ResumeSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subscriptionId, ct);

        if (subscription is null) return false;

        // STUB: In production, call PayPal activate subscription API
        _logger.LogInformation("STUB: Resuming PayPal subscription {SubId}", subscriptionId);

        subscription.Status = SubscriptionStatus.Active;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<WebhookProcessingResult> ProcessWebhookAsync(string payload, string signature, CancellationToken ct = default)
    {
        // Step 1: Verify signature (stub — validates format only)
        if (string.IsNullOrWhiteSpace(signature) || signature.Length < 10)
        {
            _logger.LogWarning("PayPal webhook signature verification failed");
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
        var eventType = root.TryGetProperty("event_type", out var typeProp) ? typeProp.GetString() : null;

        if (string.IsNullOrWhiteSpace(eventId) || string.IsNullOrWhiteSpace(eventType))
            return new WebhookProcessingResult(false, null, null, "Missing event ID or type.", false);

        // Step 3: Idempotency check
        var existingEvent = await _db.WebhookEvents
            .AnyAsync(e => e.ExternalEventId == eventId && e.Provider == PaymentProvider.PayPal, ct);

        if (existingEvent)
        {
            _logger.LogDebug("Duplicate PayPal webhook event {EventId} — skipping", eventId);
            return new WebhookProcessingResult(true, eventId, eventType, null, true);
        }

        // Step 4: Record event
        var webhookEvent = new WebhookEvent
        {
            Id = Guid.NewGuid(),
            ExternalEventId = eventId,
            Provider = PaymentProvider.PayPal,
            EventType = eventType,
            Payload = payload,
            Processed = false,
            ReceivedAt = DateTime.UtcNow
        };
        _db.WebhookEvents.Add(webhookEvent);

        // Step 5: Process by event type
        try
        {
            await ProcessPayPalEventAsync(eventType, root, ct);
            webhookEvent.Processed = true;
            webhookEvent.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Processed PayPal webhook {EventType} ({EventId})", eventType, eventId);
            return new WebhookProcessingResult(true, eventId, eventType, null, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process PayPal webhook {EventType} ({EventId})", eventType, eventId);
            await _db.SaveChangesAsync(ct); // Save the event record even if processing failed
            return new WebhookProcessingResult(false, eventId, eventType, ex.Message, false);
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(string transactionId, decimal? amount, string? reason, CancellationToken ct = default)
    {
        // STUB: In production, call PayPal Refund API
        _logger.LogInformation("STUB: Processing PayPal refund for transaction {TransactionId}, amount: {Amount}, reason: {Reason}",
            transactionId, amount, reason);

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.ExternalTransactionId == transactionId, ct);

        if (payment is null)
            return new RefundResult(false, null, "Transaction not found.");

        // Stub success
        return new RefundResult(true, $"REFUND-{Guid.NewGuid():N}"[..20], null);
    }

    public async Task<string?> SyncPlanAsync(Guid planId, CancellationToken ct = default)
    {
        // STUB: In production, call PayPal Billing Plans API
        _logger.LogInformation("STUB: Syncing plan {PlanId} to PayPal", planId);
        var stubId = $"P-STUB-{planId:N}".ToUpperInvariant()[..20];
        return await Task.FromResult(stubId);
    }

    private async Task ProcessPayPalEventAsync(string eventType, JsonElement root, CancellationToken ct)
    {
        var resource = root.TryGetProperty("resource", out var resProp) ? resProp : default;

        switch (eventType)
        {
            case "BILLING.SUBSCRIPTION.ACTIVATED":
                await HandleSubscriptionActivatedAsync(resource, ct);
                break;
            case "BILLING.SUBSCRIPTION.CANCELLED":
                await HandleSubscriptionCancelledAsync(resource, ct);
                break;
            case "BILLING.SUBSCRIPTION.PAYMENT.FAILED":
                await HandlePaymentFailedAsync(resource, ct);
                break;
            case "PAYMENT.SALE.COMPLETED":
                await HandlePaymentCompletedAsync(resource, ct);
                break;
            default:
                _logger.LogInformation("Unknown PayPal event type: {EventType} — no action taken", eventType);
                break;
        }
    }

    private async Task HandleSubscriptionActivatedAsync(JsonElement resource, CancellationToken ct)
    {
        var subId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (subId is null) return;

        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subId, ct);

        if (subscription is not null)
        {
            subscription.Status = SubscriptionStatus.Active;
            _logger.LogInformation("PayPal subscription {SubId} activated", subId);
        }
    }

    private async Task HandleSubscriptionCancelledAsync(JsonElement resource, CancellationToken ct)
    {
        var subId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (subId is null) return;

        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subId, ct);

        if (subscription is not null)
        {
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            // Access maintained until period end
            _logger.LogInformation("PayPal subscription {SubId} cancelled — access until {PeriodEnd}", subId, subscription.CurrentPeriodEnd);
        }
    }

    private async Task HandlePaymentFailedAsync(JsonElement resource, CancellationToken ct)
    {
        var subId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        if (subId is null) return;

        var subscription = await _db.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == subId, ct);

        if (subscription is not null)
        {
            subscription.Status = SubscriptionStatus.PastDue;
            subscription.GracePeriodEndsAt = DateTime.UtcNow.AddDays(subscription.Plan.GracePeriodDays);
            _logger.LogWarning("PayPal payment failed for subscription {SubId} — grace period until {GracePeriodEnd}",
                subId, subscription.GracePeriodEndsAt);
        }
    }

    private async Task HandlePaymentCompletedAsync(JsonElement resource, CancellationToken ct)
    {
        var transactionId = resource.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
        var billingAgreementId = resource.TryGetProperty("billing_agreement_id", out var baProp) ? baProp.GetString() : null;

        if (transactionId is null || billingAgreementId is null) return;

        var subscription = await _db.Subscriptions
            .FirstOrDefaultAsync(s => s.ExternalSubscriptionId == billingAgreementId, ct);

        if (subscription is null) return;

        // Parse amount
        decimal amount = 0;
        decimal fee = 0;
        var currency = Currency.GBP;

        if (resource.TryGetProperty("amount", out var amountProp))
        {
            if (amountProp.TryGetProperty("total", out var totalProp))
                decimal.TryParse(totalProp.GetString(), out amount);
            if (amountProp.TryGetProperty("currency", out var currProp))
            {
                var currStr = currProp.GetString();
                if (Enum.TryParse<Currency>(currStr, true, out var parsed))
                    currency = parsed;
            }
        }

        if (resource.TryGetProperty("transaction_fee", out var feeProp) &&
            feeProp.TryGetProperty("value", out var feeVal))
        {
            decimal.TryParse(feeVal.GetString(), out fee);
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            Amount = amount,
            Fees = fee,
            Net = amount - fee,
            Currency = currency,
            Provider = PaymentProvider.PayPal,
            ExternalTransactionId = transactionId,
            Status = "completed",
            PaidAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.Payments.Add(payment);

        // If subscription was past-due, recover it
        if (subscription.Status == SubscriptionStatus.PastDue)
        {
            subscription.Status = SubscriptionStatus.Active;
            subscription.GracePeriodEndsAt = null;
        }

        _logger.LogInformation("PayPal payment {TransactionId} completed — {Amount} {Currency}",
            transactionId, amount, currency);
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
