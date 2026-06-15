namespace AndyTipster.Domain.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TipId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Tip Tip { get; set; } = null!;
}
