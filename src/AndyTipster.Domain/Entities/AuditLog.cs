namespace AndyTipster.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string? TargetEntityId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }

    public ApplicationUser ActorUser { get; set; } = null!;
}
