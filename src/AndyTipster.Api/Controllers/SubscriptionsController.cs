using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Subscriptions.DTOs;
using AndyTipster.Application.Subscriptions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMySubscription()
    {
        var userId = GetUserId();
        var sub = await _subscriptionService.GetSelfServiceInfoAsync(userId);
        return sub is null ? NotFound(new { error = "No subscription found." }) : Ok(sub);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubscription(Guid id)
    {
        var sub = await _subscriptionService.GetSubscriptionByIdAsync(id);
        return sub is null ? NotFound() : Ok(sub);
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> UpgradePlan([FromBody] UpgradeDowngradeRequest request)
    {
        var userId = GetUserId();
        var result = await _subscriptionService.UpgradePlanAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription([FromBody] CancelSubscriptionRequest request)
    {
        var userId = GetUserId();
        var success = await _subscriptionService.CancelSubscriptionAsync(userId, request);
        return success ? Ok(new { message = "Subscription cancelled. Access maintained until period end." }) : NotFound();
    }

    [HttpGet("transactions")]
    [Authorize(Policy = "Permission:Subscriptions.View")]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionSearchDto search)
    {
        var result = await _subscriptionService.GetTransactionsAsync(search);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
