namespace AndyTipster.Domain.Entities;

public class SiteSettings
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = "AndyTipster";
    public string? Tagline { get; set; }
    public string? LogoLightUrl { get; set; }
    public string? LogoDarkUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public bool MaintenanceMode { get; set; }
    public string? MaintenanceMessage { get; set; }
    public string? AnalyticsScript { get; set; }
    public bool AnalyticsRequiresCookieConsent { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
