using AndyTipster.Application.Plans.DTOs;
using AndyTipster.Application.Plans.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly AndyTipsterDbContext _db;
    private readonly ILogger<PromoCodeService> _logger;

    public PromoCodeService(AndyTipsterDbContext db, ILogger<PromoCodeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<PromoCodeDto>> GetAllPromoCodesAsync(CancellationToken ct = default)
    {
        var codes = await _db.PromoCodes
            .Include(p => p.ApplicablePlans)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        return codes.Select(MapToDto).ToList();
    }

    public async Task<PromoCodeDto?> GetPromoCodeByIdAsync(Guid promoCodeId, CancellationToken ct = default)
    {
        var code = await _db.PromoCodes
            .Include(p => p.ApplicablePlans)
            .FirstOrDefaultAsync(p => p.Id == promoCodeId, ct);

        return code is null ? null : MapToDto(code);
    }

    public async Task<PromoCodeDto> CreatePromoCodeAsync(CreatePromoCodeDto dto, CancellationToken ct = default)
    {
        ValidateCreateDto(dto);

        var existingCode = await _db.PromoCodes.AnyAsync(p => p.Code == dto.Code.ToUpperInvariant(), ct);
        if (existingCode)
            throw new InvalidOperationException($"A promo code '{dto.Code}' already exists.");

        var promoCode = new PromoCode
        {
            Id = Guid.NewGuid(),
            Code = dto.Code.ToUpperInvariant(),
            DiscountType = dto.DiscountType.ToLowerInvariant(),
            DiscountValue = dto.DiscountValue,
            MaxUses = dto.MaxUses,
            CurrentUses = 0,
            ExpiresAt = dto.ExpiresAt,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Link applicable plans
        if (dto.ApplicablePlanIds.Count > 0)
        {
            var plans = await _db.Plans
                .Where(p => dto.ApplicablePlanIds.Contains(p.Id))
                .ToListAsync(ct);
            promoCode.ApplicablePlans = plans;
        }

        _db.PromoCodes.Add(promoCode);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created promo code {Code} ({DiscountType}: {Value})", promoCode.Code, promoCode.DiscountType, promoCode.DiscountValue);
        return (await GetPromoCodeByIdAsync(promoCode.Id, ct))!;
    }

    public async Task<PromoCodeDto> UpdatePromoCodeAsync(Guid promoCodeId, UpdatePromoCodeDto dto, CancellationToken ct = default)
    {
        var promoCode = await _db.PromoCodes
            .Include(p => p.ApplicablePlans)
            .FirstOrDefaultAsync(p => p.Id == promoCodeId, ct)
            ?? throw new KeyNotFoundException($"Promo code {promoCodeId} not found.");

        if (dto.DiscountType is not null)
        {
            if (dto.DiscountType != "percentage" && dto.DiscountType != "fixed")
                throw new ArgumentException("Discount type must be 'percentage' or 'fixed'.");
            promoCode.DiscountType = dto.DiscountType;
        }
        if (dto.DiscountValue.HasValue) promoCode.DiscountValue = dto.DiscountValue.Value;
        if (dto.MaxUses.HasValue) promoCode.MaxUses = dto.MaxUses.Value;
        if (dto.ExpiresAt.HasValue) promoCode.ExpiresAt = dto.ExpiresAt.Value;
        if (dto.IsActive.HasValue) promoCode.IsActive = dto.IsActive.Value;

        if (dto.ApplicablePlanIds is not null)
        {
            var plans = await _db.Plans
                .Where(p => dto.ApplicablePlanIds.Contains(p.Id))
                .ToListAsync(ct);
            promoCode.ApplicablePlans = plans;
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Updated promo code {PromoCodeId}", promoCodeId);
        return (await GetPromoCodeByIdAsync(promoCode.Id, ct))!;
    }

    public async Task DeletePromoCodeAsync(Guid promoCodeId, CancellationToken ct = default)
    {
        var promoCode = await _db.PromoCodes.FindAsync([promoCodeId], ct)
            ?? throw new KeyNotFoundException($"Promo code {promoCodeId} not found.");

        _db.PromoCodes.Remove(promoCode);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted promo code {PromoCodeId}", promoCodeId);
    }

    public async Task<ValidatePromoCodeResult> ValidatePromoCodeAsync(string code, Guid planId, CancellationToken ct = default)
    {
        var promoCode = await _db.PromoCodes
            .Include(p => p.ApplicablePlans)
            .FirstOrDefaultAsync(p => p.Code == code.ToUpperInvariant(), ct);

        if (promoCode is null)
            return new ValidatePromoCodeResult(false, "Promo code not found.", null, null, null, null);

        if (!promoCode.IsActive)
            return new ValidatePromoCodeResult(false, "Promo code is no longer active.", null, null, null, null);

        if (promoCode.ExpiresAt.HasValue && promoCode.ExpiresAt.Value < DateTime.UtcNow)
            return new ValidatePromoCodeResult(false, "Promo code has expired.", null, null, null, null);

        if (promoCode.CurrentUses >= promoCode.MaxUses)
            return new ValidatePromoCodeResult(false, "Promo code has reached maximum usage.", null, null, null, null);

        if (promoCode.ApplicablePlans.Count > 0 && !promoCode.ApplicablePlans.Any(p => p.Id == planId))
            return new ValidatePromoCodeResult(false, "Promo code is not applicable to this plan.", null, null, null, null);

        // Calculate discount
        var plan = await _db.Plans.FindAsync([planId], ct);
        if (plan is null)
            return new ValidatePromoCodeResult(false, "Plan not found.", null, null, null, null);

        if (!plan.PromoCodeCompatible)
            return new ValidatePromoCodeResult(false, "This plan does not accept promo codes.", null, null, null, null);

        decimal discountAmount;
        decimal discountedPrice;

        if (promoCode.DiscountType == "percentage")
        {
            discountAmount = Math.Round(plan.Price * (promoCode.DiscountValue / 100m), 2);
            discountedPrice = plan.Price - discountAmount;
        }
        else // fixed
        {
            discountAmount = promoCode.DiscountValue;
            discountedPrice = Math.Max(0, plan.Price - discountAmount);
        }

        return new ValidatePromoCodeResult(
            true,
            null,
            discountedPrice,
            discountAmount,
            promoCode.DiscountType,
            promoCode.DiscountValue
        );
    }

    public async Task IncrementUsageAsync(Guid promoCodeId, CancellationToken ct = default)
    {
        var promoCode = await _db.PromoCodes.FindAsync([promoCodeId], ct)
            ?? throw new KeyNotFoundException($"Promo code {promoCodeId} not found.");

        promoCode.CurrentUses++;
        await _db.SaveChangesAsync(ct);
    }

    private static void ValidateCreateDto(CreatePromoCodeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ArgumentException("Promo code is required.");
        if (dto.DiscountType != "percentage" && dto.DiscountType != "fixed")
            throw new ArgumentException("Discount type must be 'percentage' or 'fixed'.");
        if (dto.DiscountType == "percentage" && (dto.DiscountValue < 1 || dto.DiscountValue > 100))
            throw new ArgumentException("Percentage discount must be between 1 and 100.");
        if (dto.DiscountType == "fixed" && dto.DiscountValue <= 0)
            throw new ArgumentException("Fixed discount must be greater than 0.");
        if (dto.MaxUses < 1)
            throw new ArgumentException("Max uses must be at least 1.");
    }

    private static PromoCodeDto MapToDto(PromoCode code) => new(
        code.Id,
        code.Code,
        code.DiscountType,
        code.DiscountValue,
        code.MaxUses,
        code.CurrentUses,
        code.ExpiresAt,
        code.IsActive,
        code.ApplicablePlans.Select(p => p.Id).ToList(),
        code.CreatedAt
    );
}
