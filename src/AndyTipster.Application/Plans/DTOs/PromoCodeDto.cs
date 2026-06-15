namespace AndyTipster.Application.Plans.DTOs;

public record PromoCodeDto(
    Guid Id,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    int MaxUses,
    int CurrentUses,
    DateTime? ExpiresAt,
    bool IsActive,
    List<Guid> ApplicablePlanIds,
    DateTime CreatedAt
);

public record CreatePromoCodeDto(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    int MaxUses,
    DateTime? ExpiresAt,
    List<Guid> ApplicablePlanIds
);

public record UpdatePromoCodeDto(
    string? DiscountType,
    decimal? DiscountValue,
    int? MaxUses,
    DateTime? ExpiresAt,
    bool? IsActive,
    List<Guid>? ApplicablePlanIds
);

public record ValidatePromoCodeResult(
    bool IsValid,
    string? ErrorMessage,
    decimal? DiscountedPrice,
    decimal? DiscountAmount,
    string? DiscountType,
    decimal? DiscountValue
);
