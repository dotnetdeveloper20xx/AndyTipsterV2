using System.Security.Claims;
using AndyTipster.Application.Notifications.DTOs;
using AndyTipster.Application.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly INotificationPreferencesService _preferencesService;

    public NotificationsController(
        INotificationService notificationService,
        INotificationPreferencesService preferencesService)
    {
        _notificationService = notificationService;
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Get current user's notifications (20 most recent).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<NotificationListDto>> GetNotifications([FromQuery] int count = 20)
    {
        var userId = GetUserId();
        var result = await _notificationService.GetUserNotificationsAsync(userId, count);
        return Ok(result);
    }

    /// <summary>
    /// Get unread notification count.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { count = count > 99 ? "99+" : count.ToString(), rawCount = count });
    }

    /// <summary>
    /// Mark a single notification as read.
    /// </summary>
    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        var userId = GetUserId();
        await _notificationService.MarkAsReadAsync(notificationId, userId);
        return NoContent();
    }

    /// <summary>
    /// Mark all notifications as read.
    /// </summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Send admin broadcast to all active subscribers.
    /// </summary>
    [HttpPost("broadcast")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> SendBroadcast([FromBody] BroadcastDto dto)
    {
        await _notificationService.SendBroadcastAsync(dto);
        return Ok(new { message = "Broadcast sent successfully." });
    }

    /// <summary>
    /// Get user notification preferences.
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences()
    {
        var userId = GetUserId();
        var prefs = await _preferencesService.GetPreferencesAsync(userId);
        return Ok(prefs);
    }

    /// <summary>
    /// Update user notification preferences.
    /// </summary>
    [HttpPatch("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> UpdatePreferences([FromBody] UpdateNotificationPreferencesDto dto)
    {
        var userId = GetUserId();
        var prefs = await _preferencesService.UpdatePreferencesAsync(userId, dto);
        return Ok(prefs);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
