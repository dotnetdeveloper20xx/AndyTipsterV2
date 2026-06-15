using System.Security.Claims;
using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/cms/pages")]
[Authorize]
public class CmsController : ControllerBase
{
    private readonly IPageService _pageService;

    public CmsController(IPageService pageService)
    {
        _pageService = pageService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<List<PageDto>>> GetPages([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var pages = await _pageService.GetPagesAsync(status, page, pageSize);
        return Ok(pages);
    }

    [HttpGet("{pageId:guid}")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<PageDto>> GetPage(Guid pageId)
    {
        var result = await _pageService.GetPageByIdAsync(pageId);
        return Ok(result);
    }

    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<PageDto>> GetPageBySlug(string slug)
    {
        var result = await _pageService.GetPageBySlugAsync(slug);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:CMS.Create")]
    public async Task<ActionResult<PageDto>> CreatePage([FromBody] CreatePageRequest request)
    {
        var userId = GetUserId();
        var result = await _pageService.CreatePageAsync(request, userId);
        return CreatedAtAction(nameof(GetPage), new { pageId = result.Id }, result);
    }

    [HttpPatch("{pageId:guid}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<PageDto>> UpdatePage(Guid pageId, [FromBody] UpdatePageRequest request)
    {
        var userId = GetUserId();
        var result = await _pageService.UpdatePageAsync(pageId, request, userId);
        return Ok(result);
    }

    [HttpDelete("{pageId:guid}")]
    [Authorize(Policy = "Permission:CMS.Delete")]
    public async Task<IActionResult> DeletePage(Guid pageId)
    {
        await _pageService.DeletePageAsync(pageId);
        return NoContent();
    }

    [HttpPost("{pageId:guid}/publish")]
    [Authorize(Policy = "Permission:CMS.Publish")]
    public async Task<ActionResult<PageDto>> PublishPage(Guid pageId, [FromBody] PublishPageRequest request)
    {
        var userId = GetUserId();
        var result = await _pageService.PublishPageAsync(pageId, request, userId);
        return Ok(result);
    }

    [HttpPost("{pageId:guid}/unpublish")]
    [Authorize(Policy = "Permission:CMS.Publish")]
    public async Task<ActionResult<PageDto>> UnpublishPage(Guid pageId)
    {
        var result = await _pageService.UnpublishPageAsync(pageId);
        return Ok(result);
    }

    [HttpGet("{pageId:guid}/versions")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<List<PageVersionDto>>> GetVersionHistory(Guid pageId)
    {
        var versions = await _pageService.GetVersionHistoryAsync(pageId);
        return Ok(versions);
    }

    [HttpGet("{pageId:guid}/versions/{versionNumber:int}")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<PageVersionDto>> GetVersion(Guid pageId, int versionNumber)
    {
        var version = await _pageService.GetVersionAsync(pageId, versionNumber);
        return Ok(version);
    }

    [HttpPost("{pageId:guid}/rollback/{versionNumber:int}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<PageDto>> RollbackToVersion(Guid pageId, int versionNumber)
    {
        var userId = GetUserId();
        var result = await _pageService.RollbackToVersionAsync(pageId, versionNumber, userId);
        return Ok(result);
    }

    [HttpGet("publishing-queue")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<List<PublishingQueueItemDto>>> GetPublishingQueue()
    {
        var queue = await _pageService.GetPublishingQueueAsync();
        return Ok(queue);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
