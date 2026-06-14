using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Domain.Entities;

public class Tip
{
    public Guid Id { get; set; }
    public DateTime EventDate { get; set; }
    public string RaceName { get; set; } = string.Empty;
    public string Selection { get; set; } = string.Empty;
    public decimal Odds { get; set; }
    public int Stake { get; set; }
    public Guid CategoryId { get; set; }
    public string? Commentary { get; set; }
    public TipStatus Status { get; set; }
    public TipResult? Result { get; set; }
    public decimal? ProfitLoss { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public Guid CreatedByUserId { get; set; }

    public TipCategory Category { get; set; } = null!;
    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
