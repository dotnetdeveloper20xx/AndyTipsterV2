using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Payments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/checkout")]
[Authorize]
public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<IActionResult> InitiateCheckout([FromBody] CreateSubscriptionRequest request)
    {
        var userId = GetUserId();
        try
        {
            var session = await _checkoutService.InitiateCheckoutAsync(userId, request);
            return Ok(session);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmCheckout([FromBody] ConfirmCheckoutRequest request)
    {
        var userId = GetUserId();
        var result = await _checkoutService.ConfirmCheckoutAsync(userId, request.SessionId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("summary/{planId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCheckoutSummary(Guid planId, [FromQuery] string? promoCode)
    {
        try
        {
            var summary = await _checkoutService.GetCheckoutSummaryAsync(planId, promoCode);
            return Ok(summary);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Plan not found." });
        }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record ConfirmCheckoutRequest(string SessionId);
