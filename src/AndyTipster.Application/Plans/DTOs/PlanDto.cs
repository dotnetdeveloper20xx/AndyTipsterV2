using AndyTipster.Domain.Enumerations;

namespace AndyTipster.Application.Plans.DTOs;

public record PlanDto(
    Guid Id,
    string Name,
    string Slug,
    decimal Price,
    Currency Currency,
    BillingCycle BillingCycle,
    int TrialPeriodDays,
    decimal SetupFee,
    int GracePeriodDays,
    bool AutoRenew,
    bool PromoCodeCompatible,
    bool IsActive,
    bool IsArchived,
    string? PayPalPlanId,
    string? StripePriceId,
    PlanSyncStatus SyncStatus,
    List<string> Features,
    List<string> UpgradePaths,
    List<string> DowngradePaths,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreatePlanDto(
    string Name,
    decimal Price,
    Currency Currency,
    BillingCycle BillingCycle,
    List<string> Features,
    int TrialPeriodDays = 0,
    decimal SetupFee = 0,
    int GracePeriodDays = 7,
    bool AutoRenew = true,
    bool PromoCodeCompatible = true
);

public record UpdatePlanDto(
    string? Name,
    decimal? Price,
    Currency? Currency,
    BillingCycle? BillingCycle,
    List<string>? Features,
    int? TrialPeriodDays,
    decimal? SetupFee,
    int? GracePeriodDays,
    bool? AutoRenew,
    bool? PromoCodeCompatible
);

public record PlanTransitionPathDto(
    List<Guid> UpgradePlanIds,
    List<Guid> DowngradePlanIds
);
