using System.Security.Claims;
using AndyTipster.Application.GDPR.DTOs;
using AndyTipster.Application.GDPR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/gdpr")]
public class GdprController : ControllerBase
{
    private readonly IGdprService _gdprService;

    public GdprController(IGdprService gdprService)
    {
        _gdprService = gdprService;
    }

    // === Data Export (Right to Access) ===

    /// <summary>
    /// Request a data export (JSON or CSV archive).
    /// </summary>
    [HttpPost("export")]
    [Authorize]
    public async Task<ActionResult<DataExportStatusDto>> RequestExport([FromBody] DataExportRequestDto request)
    {
        var userId = GetUserId();
        var status = await _gdprService.RequestDataExportAsync(userId, request);
        return Ok(status);
    }

    /// <summary>
    /// Get the status of a data export request.
    /// </summary>
    [HttpGet("export/{requestId:guid}")]
    [Authorize]
    public async Task<ActionResult<DataExportStatusDto>> GetExportStatus(Guid requestId)
    {
        var userId = GetUserId();
        var status = await _gdprService.GetExportStatusAsync(userId, requestId);
        return Ok(status);
    }

    /// <summary>
    /// Download the exported data archive.
    /// </summary>
    [HttpGet("export/{requestId:guid}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadExport(Guid requestId)
    {
        var userId = GetUserId();
        var data = await _gdprService.DownloadExportAsync(userId, requestId);
        if (data == null) return NotFound();
        return File(data, "application/octet-stream", $"data-export-{requestId}.zip");
    }

    // === Account Deletion (Right to Erasure) ===

    /// <summary>
    /// Request account deletion (30-day soft delete grace period).
    /// </summary>
    [HttpPost("deletion")]
    [Authorize]
    public async Task<ActionResult<AccountDeletionStatusDto>> RequestDeletion([FromBody] AccountDeletionRequestDto request)
    {
        var userId = GetUserId();
        var status = await _gdprService.RequestAccountDeletionAsync(userId, request);
        return Ok(status);
    }

    /// <summary>
    /// Get the current deletion status.
    /// </summary>
    [HttpGet("deletion")]
    [Authorize]
    public async Task<ActionResult<AccountDeletionStatusDto>> GetDeletionStatus()
    {
        var userId = GetUserId();
        var status = await _gdprService.GetDeletionStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Cancel account deletion during grace period.
    /// </summary>
    [HttpPost("deletion/cancel")]
    [Authorize]
    public async Task<ActionResult<AccountDeletionStatusDto>> CancelDeletion()
    {
        var userId = GetUserId();
        var status = await _gdprService.CancelDeletionAsync(userId);
        return Ok(status);
    }

    // === Right to Rectification ===

    /// <summary>
    /// Update personal data (right to rectification).
    /// </summary>
    [HttpPut("rectification")]
    [Authorize]
    public async Task<IActionResult> RectifyData([FromBody] RectificationRequestDto request)
    {
        var userId = GetUserId();
        await _gdprService.RectifyPersonalDataAsync(userId, request);
        return Ok(new { message = "Personal data updated successfully." });
    }

    // === Consent Records ===

    /// <summary>
    /// Get all consent records for the current user.
    /// </summary>
    [HttpGet("consent")]
    [Authorize]
    public async Task<ActionResult<List<ConsentRecordDto>>> GetConsentRecords()
    {
        var userId = GetUserId();
        var records = await _gdprService.GetConsentRecordsAsync(userId);
        return Ok(records);
    }

    /// <summary>
    /// Record consent (grant or revoke).
    /// </summary>
    [HttpPost("consent")]
    [Authorize]
    public async Task<IActionResult> RecordConsent([FromBody] RecordConsentRequest request)
    {
        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        await _gdprService.RecordConsentAsync(userId, request.ConsentType, request.IsGranted, ipAddress, userAgent);
        return Ok(new { message = "Consent recorded." });
    }

    // === Data Processing Records ===

    /// <summary>
    /// Get data processing records for the current user.
    /// </summary>
    [HttpGet("processing-records")]
    [Authorize]
    public async Task<ActionResult<List<DataProcessingRecordDto>>> GetProcessingRecords()
    {
        var userId = GetUserId();
        var records = await _gdprService.GetProcessingRecordsAsync(userId);
        return Ok(records);
    }

    // === Breach Notification (Admin only) ===

    /// <summary>
    /// Send breach notification to affected users (Super Admin only).
    /// </summary>
    [HttpPost("breach-notification")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SendBreachNotification([FromBody] BreachNotificationDto notification)
    {
        await _gdprService.SendBreachNotificationAsync(notification);
        return Ok(new { message = "Breach notification sent." });
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}

public record RecordConsentRequest
{
    public string ConsentType { get; init; } = string.Empty;
    public bool IsGranted { get; init; }
}
