using System.Security.Claims;
using AndyTipster.Application.Telegram.DTOs;
using AndyTipster.Application.Telegram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/telegram")]
[Authorize]
public class TelegramController : ControllerBase
{
    private readonly ITelegramService _telegramService;

    public TelegramController(ITelegramService telegramService)
    {
        _telegramService = telegramService;
    }

    /// <summary>
    /// Generate a connection code for linking Telegram account.
    /// </summary>
    [HttpPost("link")]
    public async Task<ActionResult<TelegramLinkDto>> GenerateConnectionCode()
    {
        var userId = GetUserId();
        var link = await _telegramService.GenerateConnectionCodeAsync(userId);
        return Ok(link);
    }

    /// <summary>
    /// Get current Telegram link status.
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<TelegramStatusDto>> GetStatus()
    {
        var userId = GetUserId();
        var status = await _telegramService.GetLinkStatusAsync(userId);
        return Ok(status);
    }

    /// <summary>
    /// Unlink Telegram account.
    /// </summary>
    [HttpDelete("link")]
    public async Task<IActionResult> Unlink()
    {
        var userId = GetUserId();
        await _telegramService.UnlinkAccountAsync(userId);
        return NoContent();
    }

    /// <summary>
    /// Webhook endpoint for Telegram Bot API callbacks.
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] TelegramWebhookPayload payload)
    {
        if (!string.IsNullOrEmpty(payload.ConnectionCode) && !string.IsNullOrEmpty(payload.ChatId))
        {
            await _telegramService.LinkAccountAsync(payload.ConnectionCode, payload.ChatId, payload.Username ?? "");
        }

        return Ok();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}

public record TelegramWebhookPayload
{
    public string? ConnectionCode { get; init; }
    public string? ChatId { get; init; }
    public string? Username { get; init; }
}
