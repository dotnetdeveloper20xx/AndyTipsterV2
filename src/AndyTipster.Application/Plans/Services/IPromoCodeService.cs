using AndyTipster.Application.Plans.DTOs;

namespace AndyTipster.Application.Plans.Services;

/// <summary>
/// Manages promo code lifecycle and validation at checkout.
/// </summary>
public interface IPromoCodeService
{
    Task<List<PromoCodeDto>> GetAllPromoCodesAsync(CancellationToken ct = default);
    Task<PromoCodeDto?> GetPromoCodeByIdAsync(Guid promoCodeId, CancellationToken ct = default);
    Task<PromoCodeDto> CreatePromoCodeAsync(CreatePromoCodeDto dto, CancellationToken ct = default);
    Task<PromoCodeDto> UpdatePromoCodeAsync(Guid promoCodeId, UpdatePromoCodeDto dto, CancellationToken ct = default);
    Task DeletePromoCodeAsync(Guid promoCodeId, CancellationToken ct = default);
    Task<ValidatePromoCodeResult> ValidatePromoCodeAsync(string code, Guid planId, CancellationToken ct = default);
    Task IncrementUsageAsync(Guid promoCodeId, CancellationToken ct = default);
}
