using AndyTipster.Application.Payments.DTOs;
using AndyTipster.Application.Payments.Services;
using AndyTipster.Application.Subscriptions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Policy = "Permission:Analytics.View")]
public class AdminDashboardController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPayPalService _paypalService;

    public AdminDashboardController(
        ISubscriptionService subscriptionService,
        IPayPalService paypalService)
    {
        _subscriptionService = subscriptionService;
        _paypalService = paypalService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var summary = await _subscriptionService.GetAdminDashboardSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueAnalytics()
    {
        var analytics = await _subscriptionService.GetRevenueAnalyticsAsync();
        return Ok(analytics);
    }

    [HttpGet("transactions")]
    [Authorize(Policy = "Permission:Subscriptions.View")]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionSearchDto search)
    {
        var result = await _subscriptionService.GetTransactionsAsync(search);
        return Ok(result);
    }

    [HttpPost("refund")]
    [Authorize(Policy = "Permission:Subscriptions.Manage")]
    public async Task<IActionResult> ProcessRefund([FromBody] RefundRequest request)
    {
        var result = await _paypalService.ProcessRefundAsync(
            request.ExternalTransactionId, request.Amount, request.Reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
