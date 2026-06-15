using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Domain.Entities;

/// <summary>
/// Tracks processed webhook events for idempotency (deduplication by event ID).
/// </summary>
public class WebhookEvent
{
    public Guid Id { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public PaymentProvider Provider { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public bool Processed { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}
