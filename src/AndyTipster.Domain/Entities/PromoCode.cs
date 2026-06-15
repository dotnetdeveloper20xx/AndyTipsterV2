namespace AndyTipster.Domain.Entities;

public class PromoCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty; // "percentage" or "fixed"
    public decimal DiscountValue { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Plan> ApplicablePlans { get; set; } = new List<Plan>();
}
