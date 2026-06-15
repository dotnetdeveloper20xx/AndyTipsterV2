using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public decimal Fees { get; set; }
    public decimal Net { get; set; }
    public Currency Currency { get; set; }
    public PaymentProvider Provider { get; set; }
    public string ExternalTransactionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Subscription Subscription { get; set; } = null!;
}
