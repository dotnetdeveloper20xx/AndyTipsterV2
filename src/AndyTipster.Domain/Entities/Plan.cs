using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Domain.Entities;

public class Plan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Currency Currency { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public int TrialPeriodDays { get; set; }
    public decimal SetupFee { get; set; }
    public int GracePeriodDays { get; set; }
    public bool AutoRenew { get; set; }
    public bool PromoCodeCompatible { get; set; }
    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }
    public string? PayPalPlanId { get; set; }
    public string? StripePriceId { get; set; }
    public PlanSyncStatus SyncStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<TipCategory> IncludedCategories { get; set; } = new List<TipCategory>();
}
