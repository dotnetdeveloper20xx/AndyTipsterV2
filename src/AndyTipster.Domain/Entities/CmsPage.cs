using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Domain.Entities;

public class CmsPage
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool NoIndex { get; set; }
    public PageStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string BlocksJson { get; set; } = "[]";
    public int CurrentVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedByUserId { get; set; }

    public ApplicationUser CreatedByUser { get; set; } = null!;
    public ICollection<PageVersion> Versions { get; set; } = new List<PageVersion>();
    public ICollection<PageBlock> Blocks { get; set; } = new List<PageBlock>();
}
