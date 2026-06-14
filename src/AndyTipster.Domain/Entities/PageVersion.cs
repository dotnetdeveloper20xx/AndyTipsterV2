namespace AndyTipster.Domain.Entities;

public class PageVersion
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public int VersionNumber { get; set; }
    public string BlocksJson { get; set; } = "[]";
    public string? ChangeSummary { get; set; }
    public Guid AuthorUserId { get; set; }
    public DateTime CreatedAt { get; set; }

    public CmsPage Page { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
}
