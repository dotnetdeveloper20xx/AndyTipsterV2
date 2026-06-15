namespace AndyTipster.Domain.Entities;

public class PlanFeature
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public string Feature { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Plan Plan { get; set; } = null!;
}
