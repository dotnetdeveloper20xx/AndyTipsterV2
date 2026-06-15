using AndyTipster.Application.Plans.DTOs;
using AndyTipster.Application.Plans.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly ISubscriptionPlanService _planService;
    private readonly IPromoCodeService _promoService;

    public PlansController(ISubscriptionPlanService planService, IPromoCodeService promoService)
    {
        _planService = planService;
        _promoService = promoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlans([FromQuery] bool includeArchived = false)
    {
        var plans = await _planService.GetAllPlansAsync(includeArchived);
        return Ok(plans);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlan(Guid id)
    {
        var plan = await _planService.GetPlanByIdAsync(id);
        return plan is null ? NotFound() : Ok(plan);
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Plans.Create")]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto dto)
    {
        try
        {
            var plan = await _planService.CreatePlanAsync(dto);
            return CreatedAtAction(nameof(GetPlan), new { id = plan.Id }, plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "Permission:Plans.Edit")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanDto dto)
    {
        try
        {
            var plan = await _planService.UpdatePlanAsync(id, dto);
            return Ok(plan);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "Permission:Plans.Delete")]
    public async Task<IActionResult> ArchivePlan(Guid id)
    {
        try
        {
            var plan = await _planService.ArchivePlanAsync(id);
            return Ok(plan);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id:guid}/transitions")]
    [Authorize(Policy = "Permission:Plans.Edit")]
    public async Task<IActionResult> ConfigureTransitions(Guid id, [FromBody] PlanTransitionPathDto dto)
    {
        try
        {
            var plan = await _planService.ConfigureTransitionPathsAsync(id, dto);
            return Ok(plan);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/sync-paypal")]
    [Authorize(Policy = "Permission:Plans.Edit")]
    public async Task<IActionResult> SyncToPayPal(Guid id)
    {
        try
        {
            var plan = await _planService.SyncToPayPalAsync(id);
            return Ok(plan);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:guid}/retry-sync")]
    [Authorize(Policy = "Permission:Plans.Edit")]
    public async Task<IActionResult> RetrySync(Guid id)
    {
        try
        {
            var plan = await _planService.RetrySyncAsync(id);
            return Ok(plan);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // --- Promo Code Endpoints ---

    [HttpGet("promo-codes")]
    [Authorize(Policy = "Permission:Plans.View")]
    public async Task<IActionResult> GetPromoCodes()
    {
        var codes = await _promoService.GetAllPromoCodesAsync();
        return Ok(codes);
    }

    [HttpPost("promo-codes")]
    [Authorize(Policy = "Permission:Plans.Create")]
    public async Task<IActionResult> CreatePromoCode([FromBody] CreatePromoCodeDto dto)
    {
        try
        {
            var code = await _promoService.CreatePromoCodeAsync(dto);
            return CreatedAtAction(nameof(GetPromoCodes), code);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPatch("promo-codes/{id:guid}")]
    [Authorize(Policy = "Permission:Plans.Edit")]
    public async Task<IActionResult> UpdatePromoCode(Guid id, [FromBody] UpdatePromoCodeDto dto)
    {
        try
        {
            var code = await _promoService.UpdatePromoCodeAsync(id, dto);
            return Ok(code);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("promo-codes/{id:guid}")]
    [Authorize(Policy = "Permission:Plans.Delete")]
    public async Task<IActionResult> DeletePromoCode(Guid id)
    {
        try
        {
            await _promoService.DeletePromoCodeAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("promo-codes/validate")]
    public async Task<IActionResult> ValidatePromoCode([FromBody] ValidatePromoCodeRequest request)
    {
        var result = await _promoService.ValidatePromoCodeAsync(request.Code, request.PlanId);
        return Ok(result);
    }
}

public record ValidatePromoCodeRequest(string Code, Guid PlanId);
