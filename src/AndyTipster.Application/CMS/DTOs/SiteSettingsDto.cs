namespace AndyTipster.Application.CMS.DTOs;

public class SiteSettingsDto
{
    public string SiteName { get; set; } = string.Empty;
    public string? Tagline { get; set; }
    public string? LogoLightUrl { get; set; }
    public string? LogoDarkUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public string? AnalyticsScript { get; set; }
    public bool AnalyticsRequiresCookieConsent { get; set; } = true;
    public List<RedirectDto> Redirects { get; set; } = new();
}

public class UpdateSiteSettingsRequest
{
    public string? SiteName { get; set; }
    public string? Tagline { get; set; }
    public string? LogoLightUrl { get; set; }
    public string? LogoDarkUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public bool? MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public string? AnalyticsScript { get; set; }
    public bool? AnalyticsRequiresCookieConsent { get; set; }
}

public class RedirectDto
{
    public Guid Id { get; set; }
    public string FromPath { get; set; } = string.Empty;
    public string ToPath { get; set; } = string.Empty;
    public bool IsPermanent { get; set; } = true;
    public bool IsActive { get; set; } = true;
}

public class CreateRedirectRequest
{
    public string FromPath { get; set; } = string.Empty;
    public string ToPath { get; set; } = string.Empty;
    public bool IsPermanent { get; set; } = true;
}
