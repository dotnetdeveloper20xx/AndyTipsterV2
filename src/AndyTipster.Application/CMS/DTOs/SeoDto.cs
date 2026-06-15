namespace AndyTipster.Application.CMS.DTOs;

public class PageSeoDto
{
    public Guid PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool NoIndex { get; set; }
    public string? StructuredDataJson { get; set; }
}

public class UpdatePageSeoRequest
{
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public string? Slug { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool? NoIndex { get; set; }
    public string? StructuredDataJson { get; set; }
}

public class SitemapEntryDto
{
    public string Url { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
    public string ChangeFrequency { get; set; } = "weekly";
    public decimal Priority { get; set; } = 0.5m;
}
