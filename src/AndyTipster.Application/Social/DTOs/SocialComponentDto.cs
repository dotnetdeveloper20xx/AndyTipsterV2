namespace AndyTipster.Application.Social.DTOs;

public record SocialComponentDto
{
    public Guid Id { get; init; }
    public string ComponentType { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public bool IsVisible { get; init; }
    public int DisplayOrder { get; init; }
}

public record SocialFollowBarDto
{
    public List<SocialLinkDto> Links { get; init; } = new();
    public bool IsVisible { get; init; }
}

public record SocialLinkDto
{
    public string Platform { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool IsVisible { get; init; }
}

public record ShareDialogDto
{
    public string Url { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

public record OpenGraphMetaDto
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string SiteName { get; init; } = "AndyTipster";
    public string Type { get; init; } = "website";
}

public record UpdateSocialLinksDto
{
    public List<SocialLinkDto> Links { get; init; } = new();
}

public record SocialComponentVisibilityDto
{
    public string PageSlug { get; init; } = string.Empty;
    public string ComponentType { get; init; } = string.Empty;
    public bool IsVisible { get; init; }
}

public record SocialProofDto
{
    public int SubscriberCount { get; init; }
    public int TipsDelivered { get; init; }
    public double WinRate { get; init; }
}
