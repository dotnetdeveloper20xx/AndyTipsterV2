namespace AndyTipster.Domain.Entities;

public class DataProcessingRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ProcessingType { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? LegalBasis { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
