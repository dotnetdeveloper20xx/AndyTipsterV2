using System.Security.Claims;
using AndyTipster.Application.Referral.DTOs;
using AndyTipster.Application.Referral.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/referrals")]
public class ReferralsController : ControllerBase
{
    private readonly IReferralService _referralService;

    public ReferralsController(IReferralService referralService)
    {
        _referralService = referralService;
    }

    /// <summary>
    /// Get or create the current user's referral link.
    /// </summary>
    [HttpGet("link")]
    [Authorize]
    public async Task<ActionResult<ReferralLinkDto>> GetReferralLink()
    {
        var userId = GetUserId();
        var link = await _referralService.GetOrCreateReferralLinkAsync(userId);
        return Ok(link);
    }

    /// <summary>
    /// Get referral dashboard for the current user.
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize]
    public async Task<ActionResult<ReferralDashboardDto>> GetDashboard()
    {
        var userId = GetUserId();
        var dashboard = await _referralService.GetDashboardAsync(userId);
        return Ok(dashboard);
    }

    /// <summary>
    /// Track a referral click (public endpoint).
    /// </summary>
    [HttpPost("click/{referralCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> TrackClick(string referralCode)
    {
        await _referralService.TrackClickAsync(referralCode);
        return Ok(new { message = "Click tracked." });
    }

    /// <summary>
    /// Convert a referral (called when a referred user subscribes).
    /// </summary>
    [HttpPost("convert")]
    [Authorize]
    public async Task<ActionResult> ConvertReferral([FromBody] ConvertReferralDto dto)
    {
        var success = await _referralService.ConvertReferralAsync(dto);
        return Ok(new { converted = success });
    }

    /// <summary>
    /// Get referral program configuration (admin).
    /// </summary>
    [HttpGet("config")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ReferralConfigDto>> GetConfig()
    {
        var config = await _referralService.GetConfigAsync();
        return Ok(config);
    }

    /// <summary>
    /// Update referral program configuration (admin).
    /// </summary>
    [HttpPatch("config")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<ReferralConfigDto>> UpdateConfig([FromBody] UpdateReferralConfigDto dto)
    {
        var config = await _referralService.UpdateConfigAsync(dto);
        return Ok(config);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
