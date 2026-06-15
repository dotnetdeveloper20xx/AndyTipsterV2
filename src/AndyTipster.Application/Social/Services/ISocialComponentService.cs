using AndyTipster.Application.Social.DTOs;

namespace AndyTipster.Application.Social.Services;

public interface ISocialComponentService
{
    Task<SocialFollowBarDto> GetSocialFollowBarAsync();
    Task UpdateSocialLinksAsync(UpdateSocialLinksDto dto);
    Task<OpenGraphMetaDto> GetOpenGraphMetaAsync(string pageSlug);
    Task<SocialProofDto> GetSocialProofAsync();
    Task<ShareDialogDto> GetShareDialogAsync(string pageSlug);
    Task SetComponentVisibilityAsync(SocialComponentVisibilityDto dto);
    Task<List<SocialComponentVisibilityDto>> GetPageComponentVisibilityAsync(string pageSlug);
}
