using System.Security.Claims;
using AndyTipster.Application.Analytics.DTOs;
using AndyTipster.Application.Analytics.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IPublicAnalyticsService _publicAnalytics;
    private readonly ISubscriberAnalyticsService _subscriberAnalytics;
    private readonly IRevenueAnalyticsService _revenueAnalytics;

    public AnalyticsController(
        IPublicAnalyticsService publicAnalytics,
        ISubscriberAnalyticsService subscriberAnalytics,
        IRevenueAnalyticsService revenueAnalytics)
    {
        _publicAnalytics = publicAnalytics;
        _subscriberAnalytics = subscriberAnalytics;
        _revenueAnalytics = revenueAnalytics;
    }

    // === Public Performance Analytics ===

    /// <summary>
    /// Get public performance stats (accessible to everyone).
    /// </summary>
    [HttpGet("public/stats")]
    [AllowAnonymous]
    public async Task<ActionResult<PublicStatsDto>> GetPublicStats()
    {
        var stats = await _publicAnalytics.GetPublicStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// Get last 30 days summary for landing page.
    /// </summary>
    [HttpGet("public/last-30-days")]
    [AllowAnonymous]
    public async Task<ActionResult<Last30DaysSummaryDto>> GetLast30DaysSummary()
    {
        var summary = await _publicAnalytics.GetLast30DaysSummaryAsync();
        return Ok(summary);
    }

    /// <summary>
    /// Export results as CSV.
    /// </summary>
    [HttpGet("public/export/csv")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId)
    {
        var csv = await _publicAnalytics.ExportResultsCsvAsync(startDate, endDate, categoryId);
        return File(csv, "text/csv", $"andytipster-results-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    /// <summary>
    /// Export results as PDF.
    /// </summary>
    [HttpGet("public/export/pdf")]
    [AllowAnonymous]
    public async Task<IActionResult> ExportPdf(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId)
    {
        var pdf = await _publicAnalytics.ExportResultsPdfAsync(startDate, endDate, categoryId);
        return File(pdf, "application/pdf", $"andytipster-results-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    // === Subscriber Performance Dashboard ===

    /// <summary>
    /// Get personal P&L at level stakes for the subscriber.
    /// </summary>
    [HttpGet("subscriber/performance")]
    [Authorize]
    public async Task<ActionResult<SubscriberPerformanceDto>> GetSubscriberPerformance(
        [FromQuery] SubscriberPerformanceFilterDto? filter)
    {
        var userId = GetUserId();
        var performance = await _subscriberAnalytics.GetSubscriberPerformanceAsync(userId, filter);
        return Ok(performance);
    }

    /// <summary>
    /// Get monthly performance summaries for the subscriber.
    /// </summary>
    [HttpGet("subscriber/monthly")]
    [Authorize]
    public async Task<ActionResult<List<MonthlyPerformanceSummaryDto>>> GetMonthlySummaries(
        [FromQuery] int months = 12)
    {
        var userId = GetUserId();
        var summaries = await _subscriberAnalytics.GetMonthlySummariesAsync(userId, months);
        return Ok(summaries);
    }

    /// <summary>
    /// Trigger monthly digest email for the subscriber.
    /// </summary>
    [HttpPost("subscriber/digest")]
    [Authorize]
    public async Task<IActionResult> SendMonthlyDigest()
    {
        var userId = GetUserId();
        await _subscriberAnalytics.SendMonthlyDigestEmailAsync(userId);
        return Ok(new { message = "Monthly digest email sent." });
    }

    // === Admin Revenue Analytics ===

    /// <summary>
    /// Get admin revenue analytics (unified PayPal + Stripe view).
    /// </summary>
    [HttpGet("admin/revenue")]
    [Authorize(Policy = "Permission:Analytics.View")]
    public async Task<ActionResult<RevenueAnalyticsDto>> GetRevenueAnalytics(
        [FromQuery] RevenueFilterDto? filter)
    {
        var analytics = await _revenueAnalytics.GetRevenueAnalyticsAsync(filter);
        return Ok(analytics);
    }

    /// <summary>
    /// Get revenue trends with granularity (daily, weekly, monthly).
    /// </summary>
    [HttpGet("admin/revenue/trends")]
    [Authorize(Policy = "Permission:Analytics.View")]
    public async Task<ActionResult<List<RevenueTrendDto>>> GetRevenueTrends(
        [FromQuery] RevenueFilterDto filter)
    {
        var trends = await _revenueAnalytics.GetRevenueTrendsAsync(filter);
        return Ok(trends);
    }

    /// <summary>
    /// Get subscriber growth forecast.
    /// </summary>
    [HttpGet("admin/forecast")]
    [Authorize(Policy = "Permission:Analytics.View")]
    public async Task<ActionResult<SubscriberForecastDto>> GetForecast()
    {
        var forecast = await _revenueAnalytics.GetSubscriberForecastAsync();
        return Ok(forecast);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
