namespace AndyTipster.Domain.Entities;

public class DataExportRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Format { get; set; } = "json"; // "json" or "csv"
    public string Status { get; set; } = "Pending"; // Pending, Processing, Ready, Expired
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? DownloadUrl { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
