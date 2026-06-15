namespace AndyTipster.Domain.Entities;

public class PlanTransitionPath
{
    public Guid Id { get; set; }
    public Guid SourcePlanId { get; set; }
    public Guid TargetPlanId { get; set; }
    public string TransitionType { get; set; } = string.Empty; // "upgrade" or "downgrade"

    public Plan SourcePlan { get; set; } = null!;
    public Plan TargetPlan { get; set; } = null!;
}
