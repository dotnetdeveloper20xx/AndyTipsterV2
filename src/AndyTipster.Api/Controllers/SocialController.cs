using AndyTipster.Application.Social.DTOs;
using AndyTipster.Application.Social.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/social")]
public class SocialController : ControllerBase
{
    private readonly ISocialComponentService _socialService;

    public SocialController(ISocialComponentService socialService)
    {
        _socialService = socialService;
    }

    /// <summary>
    /// Get social follow bar links.
    /// </summary>
    [HttpGet("follow-bar")]
    [AllowAnonymous]
    public async Task<ActionResult<SocialFollowBarDto>> GetFollowBar()
    {
        var result = await _socialService.GetSocialFollowBarAsync();
        return Ok(result);
    }

    /// <summary>
    /// Update social media links (admin).
    /// </summary>
    [HttpPut("links")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateLinks([FromBody] UpdateSocialLinksDto dto)
    {
        await _socialService.UpdateSocialLinksAsync(dto);
        return Ok(new { message = "Social links updated." });
    }

    /// <summary>
    /// Get Open Graph meta tags for a page.
    /// </summary>
    [HttpGet("opengraph/{pageSlug}")]
    [AllowAnonymous]
    public async Task<ActionResult<OpenGraphMetaDto>> GetOpenGraphMeta(string pageSlug)
    {
        var meta = await _socialService.GetOpenGraphMetaAsync(pageSlug);
        return Ok(meta);
    }

    /// <summary>
    /// Get social proof stats (subscriber count, tips delivered, win rate).
    /// </summary>
    [HttpGet("proof")]
    [AllowAnonymous]
    public async Task<ActionResult<SocialProofDto>> GetSocialProof()
    {
        var proof = await _socialService.GetSocialProofAsync();
        return Ok(proof);
    }

    /// <summary>
    /// Get share dialog data for a page.
    /// </summary>
    [HttpGet("share/{pageSlug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ShareDialogDto>> GetShareDialog(string pageSlug)
    {
        var dialog = await _socialService.GetShareDialogAsync(pageSlug);
        return Ok(dialog);
    }

    /// <summary>
    /// Set component visibility for a page (admin).
    /// </summary>
    [HttpPost("visibility")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> SetVisibility([FromBody] SocialComponentVisibilityDto dto)
    {
        await _socialService.SetComponentVisibilityAsync(dto);
        return Ok(new { message = "Visibility updated." });
    }

    /// <summary>
    /// Get component visibility settings for a page.
    /// </summary>
    [HttpGet("visibility/{pageSlug}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SocialComponentVisibilityDto>>> GetPageVisibility(string pageSlug)
    {
        var visibility = await _socialService.GetPageComponentVisibilityAsync(pageSlug);
        return Ok(visibility);
    }
}
