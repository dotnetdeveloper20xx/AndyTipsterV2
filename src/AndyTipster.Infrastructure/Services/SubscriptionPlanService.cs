using AndyTipster.Application.Plans.DTOs;
using AndyTipster.Application.Plans.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AndyTipster.Infrastructure.Services;

public class SubscriptionPlanService : ISubscriptionPlanService
{
    private readonly AndyTipsterDbContext _db;
    private readonly ILogger<SubscriptionPlanService> _logger;

    public SubscriptionPlanService(AndyTipsterDbContext db, ILogger<SubscriptionPlanService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<PlanDto>> GetAllPlansAsync(bool includeArchived = false, CancellationToken ct = default)
    {
        var query = _db.Plans
            .Include(p => p.Features)
            .Include(p => p.UpgradePaths)
            .Include(p => p.DowngradePaths)
            .AsQueryable();

        if (!includeArchived)
            query = query.Where(p => !p.IsArchived);

        var plans = await query.OrderBy(p => p.Price).ToListAsync(ct);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<PlanDto?> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.Plans
            .Include(p => p.Features)
            .Include(p => p.UpgradePaths)
            .Include(p => p.DowngradePaths)
            .FirstOrDefaultAsync(p => p.Id == planId, ct);

        return plan is null ? null : MapToDto(plan);
    }

    public async Task<PlanDto> CreatePlanAsync(CreatePlanDto dto, CancellationToken ct = default)
    {
        ValidateCreateDto(dto);

        var existingName = await _db.Plans.AnyAsync(p => p.Name == dto.Name && !p.IsArchived, ct);
        if (existingName)
            throw new InvalidOperationException($"A plan with name '{dto.Name}' already exists.");

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Slug = GenerateSlug(dto.Name),
            Price = dto.Price,
            Currency = dto.Currency,
            BillingCycle = dto.BillingCycle,
            TrialPeriodDays = dto.TrialPeriodDays,
            SetupFee = dto.SetupFee,
            GracePeriodDays = dto.GracePeriodDays,
            AutoRenew = dto.AutoRenew,
            PromoCodeCompatible = dto.PromoCodeCompatible,
            IsActive = true,
            IsArchived = false,
            SyncStatus = PlanSyncStatus.SyncPending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Plans.Add(plan);

        // Add features
        for (int i = 0; i < dto.Features.Count; i++)
        {
            _db.PlanFeatures.Add(new PlanFeature
            {
                Id = Guid.NewGuid(),
                PlanId = plan.Id,
                Feature = dto.Features[i],
                SortOrder = i
            });
        }

        await _db.SaveChangesAsync(ct);

        // Attempt PayPal sync (stub)
        await TrySyncToPayPalAsync(plan, ct);

        _logger.LogInformation("Created plan {PlanId} '{PlanName}'", plan.Id, plan.Name);
        return (await GetPlanByIdAsync(plan.Id, ct))!;
    }

    public async Task<PlanDto> UpdatePlanAsync(Guid planId, UpdatePlanDto dto, CancellationToken ct = default)
    {
        var plan = await _db.Plans
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == planId, ct)
            ?? throw new KeyNotFoundException($"Plan {planId} not found.");

        if (dto.Name is not null)
        {
            if (dto.Name.Length < 1 || dto.Name.Length > 100)
                throw new ArgumentException("Plan name must be 1-100 characters.");

            var existingName = await _db.Plans.AnyAsync(p => p.Name == dto.Name && p.Id != planId && !p.IsArchived, ct);
            if (existingName)
                throw new InvalidOperationException($"A plan with name '{dto.Name}' already exists.");

            plan.Name = dto.Name;
            plan.Slug = GenerateSlug(dto.Name);
        }

        if (dto.Price.HasValue)
        {
            if (dto.Price.Value < 0.01m || dto.Price.Value > 999_999.99m)
                throw new ArgumentException("Price must be between 0.01 and 999,999.99.");
            plan.Price = dto.Price.Value;
        }

        if (dto.Currency.HasValue) plan.Currency = dto.Currency.Value;
        if (dto.BillingCycle.HasValue) plan.BillingCycle = dto.BillingCycle.Value;
        if (dto.TrialPeriodDays.HasValue)
        {
            if (dto.TrialPeriodDays.Value < 0 || dto.TrialPeriodDays.Value > 365)
                throw new ArgumentException("Trial period must be 0-365 days.");
            plan.TrialPeriodDays = dto.TrialPeriodDays.Value;
        }
        if (dto.SetupFee.HasValue)
        {
            if (dto.SetupFee.Value < 0 || dto.SetupFee.Value > 999_999.99m)
                throw new ArgumentException("Setup fee must be 0.00-999,999.99.");
            plan.SetupFee = dto.SetupFee.Value;
        }
        if (dto.GracePeriodDays.HasValue)
        {
            if (dto.GracePeriodDays.Value < 0 || dto.GracePeriodDays.Value > 90)
                throw new ArgumentException("Grace period must be 0-90 days.");
            plan.GracePeriodDays = dto.GracePeriodDays.Value;
        }
        if (dto.AutoRenew.HasValue) plan.AutoRenew = dto.AutoRenew.Value;
        if (dto.PromoCodeCompatible.HasValue) plan.PromoCodeCompatible = dto.PromoCodeCompatible.Value;

        if (dto.Features is not null)
        {
            if (dto.Features.Count < 1 || dto.Features.Count > 50)
                throw new ArgumentException("Features list must contain 1-50 items.");

            _db.PlanFeatures.RemoveRange(plan.Features);
            for (int i = 0; i < dto.Features.Count; i++)
            {
                _db.PlanFeatures.Add(new PlanFeature
                {
                    Id = Guid.NewGuid(),
                    PlanId = plan.Id,
                    Feature = dto.Features[i],
                    SortOrder = i
                });
            }
        }

        plan.UpdatedAt = DateTime.UtcNow;
        plan.SyncStatus = PlanSyncStatus.SyncPending;
        await _db.SaveChangesAsync(ct);

        await TrySyncToPayPalAsync(plan, ct);

        _logger.LogInformation("Updated plan {PlanId}", planId);
        return (await GetPlanByIdAsync(plan.Id, ct))!;
    }

    public async Task<PlanDto> ArchivePlanAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([planId], ct)
            ?? throw new KeyNotFoundException($"Plan {planId} not found.");

        plan.IsArchived = true;
        plan.IsActive = false;
        plan.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Archived plan {PlanId} — existing subscribers unaffected", planId);
        return (await GetPlanByIdAsync(plan.Id, ct))!;
    }

    public async Task<PlanDto> ConfigureTransitionPathsAsync(Guid planId, PlanTransitionPathDto dto, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([planId], ct)
            ?? throw new KeyNotFoundException($"Plan {planId} not found.");

        // Remove existing paths
        var existingPaths = await _db.PlanTransitionPaths
            .Where(p => p.SourcePlanId == planId)
            .ToListAsync(ct);
        _db.PlanTransitionPaths.RemoveRange(existingPaths);

        // Add upgrade paths
        foreach (var targetId in dto.UpgradePlanIds)
        {
            _db.PlanTransitionPaths.Add(new PlanTransitionPath
            {
                Id = Guid.NewGuid(),
                SourcePlanId = planId,
                TargetPlanId = targetId,
                TransitionType = "upgrade"
            });
        }

        // Add downgrade paths
        foreach (var targetId in dto.DowngradePlanIds)
        {
            _db.PlanTransitionPaths.Add(new PlanTransitionPath
            {
                Id = Guid.NewGuid(),
                SourcePlanId = planId,
                TargetPlanId = targetId,
                TransitionType = "downgrade"
            });
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Configured transition paths for plan {PlanId}", planId);
        return (await GetPlanByIdAsync(plan.Id, ct))!;
    }

    public async Task<PlanDto> SyncToPayPalAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.Plans.FindAsync([planId], ct)
            ?? throw new KeyNotFoundException($"Plan {planId} not found.");

        await TrySyncToPayPalAsync(plan, ct);
        return (await GetPlanByIdAsync(plan.Id, ct))!;
    }

    public async Task<PlanDto> RetrySyncAsync(Guid planId, CancellationToken ct = default)
    {
        return await SyncToPayPalAsync(planId, ct);
    }

    private async Task TrySyncToPayPalAsync(Plan plan, CancellationToken ct)
    {
        try
        {
            // STUB: In production, this would call PayPal Billing Plans API
            _logger.LogInformation("STUB: Syncing plan {PlanId} to PayPal Billing Plans API", plan.Id);

            // Simulate sync — in real implementation, this would make HTTP call
            plan.PayPalPlanId = $"P-STUB-{plan.Id:N}".ToUpperInvariant()[..20];
            plan.SyncStatus = PlanSyncStatus.Synced;
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to sync plan {PlanId} to PayPal — marked as SyncPending", plan.Id);
            plan.SyncStatus = PlanSyncStatus.SyncFailed;
            await _db.SaveChangesAsync(ct);
        }
    }

    private static void ValidateCreateDto(CreatePlanDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 100)
            throw new ArgumentException("Plan name must be 1-100 characters.");
        if (dto.Price < 0.01m || dto.Price > 999_999.99m)
            throw new ArgumentException("Price must be between 0.01 and 999,999.99.");
        if (dto.Features.Count < 1 || dto.Features.Count > 50)
            throw new ArgumentException("Features list must contain 1-50 items.");
        if (dto.TrialPeriodDays < 0 || dto.TrialPeriodDays > 365)
            throw new ArgumentException("Trial period must be 0-365 days.");
        if (dto.SetupFee < 0 || dto.SetupFee > 999_999.99m)
            throw new ArgumentException("Setup fee must be 0.00-999,999.99.");
        if (dto.GracePeriodDays < 0 || dto.GracePeriodDays > 90)
            throw new ArgumentException("Grace period must be 0-90 days.");
    }

    private static string GenerateSlug(string name) =>
        name.ToLowerInvariant()
            .Replace(' ', '-')
            .Replace("--", "-")
            .Trim('-');

    private static PlanDto MapToDto(Plan plan) => new(
        plan.Id,
        plan.Name,
        plan.Slug,
        plan.Price,
        plan.Currency,
        plan.BillingCycle,
        plan.TrialPeriodDays,
        plan.SetupFee,
        plan.GracePeriodDays,
        plan.AutoRenew,
        plan.PromoCodeCompatible,
        plan.IsActive,
        plan.IsArchived,
        plan.PayPalPlanId,
        plan.StripePriceId,
        plan.SyncStatus,
        plan.Features.OrderBy(f => f.SortOrder).Select(f => f.Feature).ToList(),
        plan.UpgradePaths.Select(p => p.TargetPlanId.ToString()).ToList(),
        plan.DowngradePaths.Select(p => p.TargetPlanId.ToString()).ToList(),
        plan.CreatedAt,
        plan.UpdatedAt
    );
}
