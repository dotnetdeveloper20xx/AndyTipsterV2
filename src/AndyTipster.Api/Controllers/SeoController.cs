using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/seo")]
public class SeoController : ControllerBase
{
    private readonly ISeoService _seoService;

    public SeoController(ISeoService seoService)
    {
        _seoService = seoService;
    }

    [HttpGet("pages/{pageId:guid}")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<PageSeoDto>> GetPageSeo(Guid pageId)
    {
        var result = await _seoService.GetPageSeoAsync(pageId);
        return Ok(result);
    }

    [HttpPatch("pages/{pageId:guid}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<PageSeoDto>> UpdatePageSeo(Guid pageId, [FromBody] UpdatePageSeoRequest request)
    {
        var result = await _seoService.UpdatePageSeoAsync(pageId, request);
        return Ok(result);
    }

    [HttpGet("sitemap.xml")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSitemap()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var xml = await _seoService.GenerateSitemapXmlAsync(baseUrl);
        return Content(xml, "application/xml");
    }

    [HttpGet("pages/{pageId:guid}/structured-data")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> GetStructuredData(Guid pageId)
    {
        var jsonLd = await _seoService.GenerateStructuredDataAsync(pageId);
        return Content(jsonLd, "application/ld+json");
    }
}
