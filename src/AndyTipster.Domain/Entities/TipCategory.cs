namespace AndyTipster.Domain.Entities;

public class TipCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Tip> Tips { get; set; } = new List<Tip>();
    public ICollection<Plan> Plans { get; set; } = new List<Plan>();
}
