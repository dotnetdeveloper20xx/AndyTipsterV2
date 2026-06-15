using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/site-settings")]
public class SiteSettingsController : ControllerBase
{
    private readonly ISiteSettingsService _siteSettingsService;

    public SiteSettingsController(ISiteSettingsService siteSettingsService)
    {
        _siteSettingsService = siteSettingsService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<SiteSettingsDto>> GetSettings()
    {
        var settings = await _siteSettingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPatch]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<SiteSettingsDto>> UpdateSettings([FromBody] UpdateSiteSettingsRequest request)
    {
        var settings = await _siteSettingsService.UpdateSettingsAsync(request);
        return Ok(settings);
    }

    [HttpGet("maintenance-mode")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> IsMaintenanceMode()
    {
        var isActive = await _siteSettingsService.IsMaintenanceModeAsync();
        return Ok(isActive);
    }

    [HttpGet("redirects")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<List<RedirectDto>>> GetRedirects()
    {
        var redirects = await _siteSettingsService.GetRedirectsAsync();
        return Ok(redirects);
    }

    [HttpPost("redirects")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<RedirectDto>> CreateRedirect([FromBody] CreateRedirectRequest request)
    {
        var redirect = await _siteSettingsService.CreateRedirectAsync(request);
        return Ok(redirect);
    }

    [HttpPut("redirects/{redirectId:guid}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<RedirectDto>> UpdateRedirect(Guid redirectId, [FromBody] CreateRedirectRequest request)
    {
        var redirect = await _siteSettingsService.UpdateRedirectAsync(redirectId, request);
        return Ok(redirect);
    }

    [HttpDelete("redirects/{redirectId:guid}")]
    [Authorize(Policy = "Permission:CMS.Delete")]
    public async Task<IActionResult> DeleteRedirect(Guid redirectId)
    {
        await _siteSettingsService.DeleteRedirectAsync(redirectId);
        return NoContent();
    }
}
