namespace AndyTipster.Application.Blog.DTOs;

public record BlogPostDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime? ScheduledPublishAt { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
}

public record CreateBlogPostDto
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public record UpdateBlogPostDto
{
    public string? Title { get; init; }
    public string? Content { get; init; }
    public string? Excerpt { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string? MetaTitle { get; init; }
    public string? MetaDescription { get; init; }
}

public record PublishBlogPostRequest
{
    public DateTime? ScheduledPublishAt { get; init; }
}

public record BlogPostListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Excerpt { get; init; }
    public string? FeaturedImageUrl { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? PublishedAt { get; init; }
    public string AuthorName { get; init; } = string.Empty;
}
