using AndyTipster.Application.Plans.DTOs;

namespace AndyTipster.Application.Plans.Services;

/// <summary>
/// Manages subscription plan lifecycle including CRUD, archiving, sync, and transition paths.
/// </summary>
public interface ISubscriptionPlanService
{
    Task<List<PlanDto>> GetAllPlansAsync(bool includeArchived = false, CancellationToken ct = default);
    Task<PlanDto?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default);
    Task<PlanDto> CreatePlanAsync(CreatePlanDto dto, CancellationToken ct = default);
    Task<PlanDto> UpdatePlanAsync(Guid planId, UpdatePlanDto dto, CancellationToken ct = default);
    Task<PlanDto> ArchivePlanAsync(Guid planId, CancellationToken ct = default);
    Task<PlanDto> ConfigureTransitionPathsAsync(Guid planId, PlanTransitionPathDto dto, CancellationToken ct = default);
    Task<PlanDto> SyncToPayPalAsync(Guid planId, CancellationToken ct = default);
    Task<PlanDto> RetrySyncAsync(Guid planId, CancellationToken ct = default);
}
