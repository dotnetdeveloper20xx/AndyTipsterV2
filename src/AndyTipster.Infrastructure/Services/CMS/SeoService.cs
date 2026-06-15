using System.Text;
using System.Text.Json;
using AndyTipster.Application.CMS.DTOs;
using AndyTipster.Application.CMS.Services;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services.CMS;

public class SeoService : ISeoService
{
    private readonly AndyTipsterDbContext _db;

    public SeoService(AndyTipsterDbContext db)
    {
        _db = db;
    }

    public async Task<PageSeoDto> GetPageSeoAsync(Guid pageId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        return new PageSeoDto
        {
            PageId = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            OgImageUrl = page.OgImageUrl,
            CanonicalUrl = page.CanonicalUrl,
            NoIndex = page.NoIndex
        };
    }

    public async Task<PageSeoDto> UpdatePageSeoAsync(Guid pageId, UpdatePageSeoRequest request)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        if (request.MetaTitle != null)
        {
            if (request.MetaTitle.Length > 60)
                throw new InvalidOperationException("Meta title must not exceed 60 characters.");
            page.MetaTitle = request.MetaTitle;
        }

        if (request.MetaDescription != null)
        {
            if (request.MetaDescription.Length > 160)
                throw new InvalidOperationException("Meta description must not exceed 160 characters.");
            page.MetaDescription = request.MetaDescription;
        }

        if (request.OgImageUrl != null) page.OgImageUrl = request.OgImageUrl;
        if (request.Slug != null) page.Slug = request.Slug;
        if (request.CanonicalUrl != null) page.CanonicalUrl = request.CanonicalUrl;
        if (request.NoIndex.HasValue) page.NoIndex = request.NoIndex.Value;

        page.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new PageSeoDto
        {
            PageId = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            MetaTitle = page.MetaTitle,
            MetaDescription = page.MetaDescription,
            OgImageUrl = page.OgImageUrl,
            CanonicalUrl = page.CanonicalUrl,
            NoIndex = page.NoIndex,
            StructuredDataJson = request.StructuredDataJson
        };
    }

    public async Task<string> GenerateSitemapXmlAsync(string baseUrl)
    {
        var publishedPages = await _db.CmsPages
            .Where(p => p.Status == PageStatus.Published && !p.NoIndex)
            .Select(p => new { p.Slug, p.UpdatedAt, p.CreatedAt })
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Add homepage
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>1.0</priority>");
        sb.AppendLine("  </url>");

        foreach (var page in publishedPages)
        {
            var lastMod = page.UpdatedAt ?? page.CreatedAt;
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/{page.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{lastMod:yyyy-MM-dd}</lastmod>");
            sb.AppendLine("    <changefreq>weekly</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    public async Task<string> GenerateStructuredDataAsync(Guid pageId)
    {
        var page = await _db.CmsPages.FirstOrDefaultAsync(p => p.Id == pageId)
            ?? throw new KeyNotFoundException($"Page {pageId} not found");

        var structuredData = new
        {
            @context = "https://schema.org",
            @type = "WebPage",
            name = page.MetaTitle ?? page.Title,
            description = page.MetaDescription ?? "",
            url = $"/{page.Slug}",
            datePublished = page.PublishedAt?.ToString("yyyy-MM-dd"),
            dateModified = (page.UpdatedAt ?? page.CreatedAt).ToString("yyyy-MM-dd")
        };

        return JsonSerializer.Serialize(structuredData, new JsonSerializerOptions { WriteIndented = true });
    }
}
