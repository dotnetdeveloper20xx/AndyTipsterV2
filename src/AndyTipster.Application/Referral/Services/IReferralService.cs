using AndyTipster.Application.Referral.DTOs;

namespace AndyTipster.Application.Referral.Services;

public interface IReferralService
{
    Task<ReferralLinkDto> GetOrCreateReferralLinkAsync(Guid userId);
    Task<ReferralDashboardDto> GetDashboardAsync(Guid userId);
    Task TrackClickAsync(string referralCode);
    Task<bool> ConvertReferralAsync(ConvertReferralDto dto);
    Task<ReferralConfigDto> GetConfigAsync();
    Task<ReferralConfigDto> UpdateConfigAsync(UpdateReferralConfigDto dto);
}
