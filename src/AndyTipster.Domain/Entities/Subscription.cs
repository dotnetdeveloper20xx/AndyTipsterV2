using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid PlanId { get; set; }
    public PaymentProvider Provider { get; set; }
    public string ExternalSubscriptionId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime? GracePeriodEndsAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
