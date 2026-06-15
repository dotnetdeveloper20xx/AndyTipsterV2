namespace AndyTipster.Domain.Entities;

public class PageBlock
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string BlockType { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "{}";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public CmsPage Page { get; set; } = null!;
}
