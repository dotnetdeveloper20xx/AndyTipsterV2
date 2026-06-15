namespace AndyTipster.Application.CMS.DTOs;

public class PageDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool NoIndex { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int CurrentVersion { get; set; }
    public List<BlockDto> Blocks { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
}

public class BlockDto
{
    public Guid Id { get; set; }
    public string BlockType { get; set; } = string.Empty;
    public string ContentJson { get; set; } = "{}";
    public int SortOrder { get; set; }
}

public class CreatePageRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool NoIndex { get; set; }
    public List<BlockDto> Blocks { get; set; } = new();
}

public class UpdatePageRequest
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool? NoIndex { get; set; }
    public List<BlockDto>? Blocks { get; set; }
    public string? ChangeSummary { get; set; }
}

public class PublishPageRequest
{
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Timezone { get; set; }
}

public class PageVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string BlocksJson { get; set; } = "[]";
    public string? ChangeSummary { get; set; }
    public string AuthorUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PublishingQueueItemDto
{
    public Guid PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime? ScheduledPublishAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
