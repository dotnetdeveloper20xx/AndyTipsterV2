using AndyTipster.Application.CMS.DTOs;

namespace AndyTipster.Application.CMS.Services;

public interface ISiteSettingsService
{
    Task<SiteSettingsDto> GetSettingsAsync();
    Task<SiteSettingsDto> UpdateSettingsAsync(UpdateSiteSettingsRequest request);
    Task<List<RedirectDto>> GetRedirectsAsync();
    Task<RedirectDto> CreateRedirectAsync(CreateRedirectRequest request);
    Task<RedirectDto> UpdateRedirectAsync(Guid redirectId, CreateRedirectRequest request);
    Task DeleteRedirectAsync(Guid redirectId);
    Task<bool> IsMaintenanceModeAsync();
}
