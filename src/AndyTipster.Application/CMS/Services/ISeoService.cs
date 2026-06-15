using AndyTipster.Application.CMS.DTOs;

namespace AndyTipster.Application.CMS.Services;

public interface ISeoService
{
    Task<PageSeoDto> GetPageSeoAsync(Guid pageId);
    Task<PageSeoDto> UpdatePageSeoAsync(Guid pageId, UpdatePageSeoRequest request);
    Task<string> GenerateSitemapXmlAsync(string baseUrl);
    Task<string> GenerateStructuredDataAsync(Guid pageId);
}
