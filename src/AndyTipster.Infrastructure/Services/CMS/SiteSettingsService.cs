using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.CMS;

public class SiteSettingsService : ISiteSettingsService
{
    private readonly AndyTipsterDbContext _db;

    public SiteSettingsService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<SiteSettingsDto> GetSettingsAsync()
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            // Create default settings
            settings = new SiteSettings
            {
                Id = Guid.NewGuid(),
                SiteName = "AndyTipster",
                Tagline = "UK & Ireland Horse Racing Tips",
                MaintenanceMode = false,
                AnalyticsRequiresCookieConsent = true,
                CreatedAt = DateTime.UtcNow
            };
            _db.SiteSettings.Add(settings);
            await _db.SaveChangesAsync();
        }

        var redirects = await _db.Redirects.Where(r => r.IsActive).ToListAsync();

        return MapToDto(settings, redirects);
    }

    public async Task<SiteSettingsDto> UpdateSettingsAsync(UpdateSiteSettingsRequest request)
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new SiteSettings
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };
            _db.SiteSettings.Add(settings);
        }

        if (request.SiteName != null) settings.SiteName = request.SiteName;
        if (request.Tagline != null) settings.Tagline = request.Tagline;
        if (request.LogoLightUrl != null) settings.LogoLightUrl = request.LogoLightUrl;
        if (request.LogoDarkUrl != null) settings.LogoDarkUrl = request.LogoDarkUrl;
        if (request.FaviconUrl != null) settings.FaviconUrl = request.FaviconUrl;
        if (request.MaintenanceMode.HasValue) settings.MaintenanceMode = request.MaintenanceMode.Value;
        if (request.MaintenanceMessage != null) settings.MaintenanceMessage = request.MaintenanceMessage;
        if (request.AnalyticsScript != null) settings.AnalyticsScript = request.AnalyticsScript;
        if (request.AnalyticsRequiresCookieConsent.HasValue) settings.AnalyticsRequiresCookieConsent = request.AnalyticsRequiresCookieConsent.Value;

        settings.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var redirects = await _db.Redirects.Where(r => r.IsActive).ToListAsync();
        return MapToDto(settings, redirects);
    }

    public async Task<List<RedirectDto>> GetRedirectsAsync()
    {
        var redirects = await _db.Redirects.OrderBy(r => r.FromPath).ToListAsync();
        return redirects.Select(MapRedirectToDto).ToList();
    }

    public async Task<RedirectDto> CreateRedirectAsync(CreateRedirectRequest request)
    {
        var redirect = new Redirect
        {
            Id = Guid.NewGuid(),
            FromPath = request.FromPath,
            ToPath = request.ToPath,
            IsPermanent = request.IsPermanent,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Redirects.Add(redirect);
        await _db.SaveChangesAsync();

        return MapRedirectToDto(redirect);
    }

    public async Task<RedirectDto> UpdateRedirectAsync(Guid redirectId, CreateRedirectRequest request)
    {
        var redirect = await _db.Redirects.FirstOrDefaultAsync(r => r.Id == redirectId)
            ?? throw new KeyNotFoundException($"Redirect {redirectId} not found");

        redirect.FromPath = request.FromPath;
        redirect.ToPath = request.ToPath;
        redirect.IsPermanent = request.IsPermanent;
        redirect.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapRedirectToDto(redirect);
    }

    public async Task DeleteRedirectAsync(Guid redirectId)
    {
        var redirect = await _db.Redirects.FirstOrDefaultAsync(r => r.Id == redirectId)
            ?? throw new KeyNotFoundException($"Redirect {redirectId} not found");

        _db.Redirects.Remove(redirect);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> IsMaintenanceModeAsync()
    {
        var settings = await _db.SiteSettings.FirstOrDefaultAsync();
        return settings?.MaintenanceMode ?? false;
    }

    private static SiteSettingsDto MapToDto(SiteSettings settings, List<Redirect> redirects)
    {
        return new SiteSettingsDto
        {
            SiteName = settings.SiteName,
            Tagline = settings.Tagline,
            LogoLightUrl = settings.LogoLightUrl,
            LogoDarkUrl = settings.LogoDarkUrl,
            FaviconUrl = settings.FaviconUrl,
            MaintenanceMode = settings.MaintenanceMode,
            MaintenanceMessage = settings.MaintenanceMessage,
            AnalyticsScript = settings.AnalyticsScript,
            AnalyticsRequiresCookieConsent = settings.AnalyticsRequiresCookieConsent,
            Redirects = redirects.Select(MapRedirectToDto).ToList()
        };
    }

    private static RedirectDto MapRedirectToDto(Redirect redirect)
    {
        return new RedirectDto
        {
            Id = redirect.Id,
            FromPath = redirect.FromPath,
            ToPath = redirect.ToPath,
            IsPermanent = redirect.IsPermanent,
            IsActive = redirect.IsActive
        };
    }
}
