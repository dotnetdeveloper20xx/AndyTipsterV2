using System.Security.Claims;
using AndyTipster.Application.Tips.DTOs;
using AndyTipster.Application.Tips.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/tips")]
public class TipsController : ControllerBase
{
    private readonly ITipService _tipService;
    private readonly IAccessGatingService _accessGatingService;

    public TipsController(ITipService tipService, IAccessGatingService accessGatingService)
    {
        _tipService = tipService;
        _accessGatingService = accessGatingService;
    }

    /// <summary>
    /// Get tips (admin view — all tips regardless of status).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Permission:Tips.View")]
    public async Task<ActionResult> GetTips([FromQuery] TipFilterDto filter)
    {
        var (items, totalCount) = await _tipService.GetTipsAsync(filter);
        return Ok(new { items, totalCount });
    }

    /// <summary>
    /// Get published tips accessible to the current user (subscriber feed).
    /// </summary>
    [HttpGet("feed")]
    [Authorize]
    public async Task<ActionResult> GetTipsFeed([FromQuery] TipFilterDto filter)
    {
        var userId = GetUserId();
        var (items, totalCount) = await _accessGatingService.GetAccessibleTipsAsync(userId, filter);
        return Ok(new { items, totalCount });
    }

    /// <summary>
    /// Get the Tip of the Day (public, free preview).
    /// </summary>
    [HttpGet("tip-of-the-day")]
    [AllowAnonymous]
    public async Task<ActionResult> GetTipOfTheDay()
    {
        var tip = await _accessGatingService.GetTipOfTheDayAsync();
        if (tip == null) return NotFound();
        return Ok(tip);
    }

    /// <summary>
    /// Get a specific tip by ID. Access-gated for subscribers.
    /// </summary>
    [HttpGet("{tipId:guid}")]
    [Authorize]
    public async Task<ActionResult<TipDto>> GetTip(Guid tipId)
    {
        var userId = GetUserId();
        var accessResult = await _accessGatingService.CheckAccessAsync(userId, tipId);

        if (!accessResult.HasAccess)
        {
            if (accessResult.ShowPaywall)
                return StatusCode(403, new { message = accessResult.DenialReason, showPaywall = true });
            return Forbid();
        }

        var tip = await _tipService.GetTipByIdAsync(tipId);
        return Ok(tip);
    }

    /// <summary>
    /// Create a new tip (admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Tips.Create")]
    public async Task<ActionResult<TipDto>> CreateTip([FromBody] CreateTipDto dto)
    {
        var userId = GetUserId();
        var tip = await _tipService.CreateTipAsync(dto, userId);
        return CreatedAtAction(nameof(GetTip), new { tipId = tip.Id }, tip);
    }

    /// <summary>
    /// Update a draft tip.
    /// </summary>
    [HttpPatch("{tipId:guid}")]
    [Authorize(Policy = "Permission:Tips.Edit")]
    public async Task<ActionResult<TipDto>> UpdateTip(Guid tipId, [FromBody] UpdateTipDto dto)
    {
        var tip = await _tipService.UpdateTipAsync(tipId, dto);
        return Ok(tip);
    }

    /// <summary>
    /// Delete a tip.
    /// </summary>
    [HttpDelete("{tipId:guid}")]
    [Authorize(Policy = "Permission:Tips.Delete")]
    public async Task<IActionResult> DeleteTip(Guid tipId)
    {
        await _tipService.DeleteTipAsync(tipId);
        return NoContent();
    }

    /// <summary>
    /// Publish a tip (optionally schedule for future).
    /// </summary>
    [HttpPost("{tipId:guid}/publish")]
    [Authorize(Policy = "Permission:Tips.Edit")]
    public async Task<ActionResult<TipDto>> PublishTip(Guid tipId, [FromBody] PublishTipRequest? request)
    {
        var tip = await _tipService.PublishTipAsync(tipId, request?.ScheduledPublishAt);
        return Ok(tip);
    }

    /// <summary>
    /// Archive a published tip.
    /// </summary>
    [HttpPost("{tipId:guid}/archive")]
    [Authorize(Policy = "Permission:Tips.Edit")]
    public async Task<ActionResult<TipDto>> ArchiveTip(Guid tipId)
    {
        var tip = await _tipService.ArchiveTipAsync(tipId);
        return Ok(tip);
    }

    /// <summary>
    /// Record a result for a published tip (Won, Lost, Void, Push).
    /// </summary>
    [HttpPost("{tipId:guid}/result")]
    [Authorize(Policy = "Permission:Tips.Edit")]
    public async Task<ActionResult<TipDto>> RecordResult(Guid tipId, [FromBody] RecordResultRequest request)
    {
        var tip = await _tipService.RecordResultAsync(tipId, request.Result);
        return Ok(tip);
    }

    /// <summary>
    /// Get P&L summary with optional filters and grouping.
    /// </summary>
    [HttpGet("pnl")]
    [Authorize(Policy = "Permission:Tips.View")]
    public async Task<ActionResult<PnLSummaryDto>> GetPnLSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId,
        [FromQuery] string groupBy = "month")
    {
        var summary = await _tipService.GetPnLSummaryAsync(startDate, endDate, categoryId, groupBy);
        return Ok(summary);
    }

    /// <summary>
    /// Bulk import tips from CSV file.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Policy = "Permission:Tips.Create")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max
    public async Task<ActionResult<BulkImportResultDto>> BulkImport(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File size exceeds 5MB limit." });

        var userId = GetUserId();
        using var stream = file.OpenReadStream();
        var result = await _tipService.BulkImportAsync(stream, userId);
        return Ok(result);
    }

    /// <summary>
    /// Check access for a specific tip (used by frontend for gating UI).
    /// </summary>
    [HttpGet("{tipId:guid}/access")]
    [AllowAnonymous]
    public async Task<ActionResult> CheckAccess(Guid tipId)
    {
        var userId = GetUserIdOrEmpty();
        var result = await _accessGatingService.CheckAccessAsync(userId, tipId);
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }

    private Guid GetUserIdOrEmpty()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId != null ? Guid.Parse(userId) : Guid.Empty;
    }
}
