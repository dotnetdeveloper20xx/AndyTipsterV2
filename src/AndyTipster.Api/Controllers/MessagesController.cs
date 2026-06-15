using System.Security.Claims;
using AndyTipster.Application.Community.DTOs;
using AndyTipster.Application.Community.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagesController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    /// <summary>
    /// Send a direct message.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageDto dto)
    {
        var userId = GetUserId();
        var message = await _messagingService.SendMessageAsync(dto, userId);
        return Ok(message);
    }

    /// <summary>
    /// Get conversations list.
    /// </summary>
    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationDto>>> GetConversations()
    {
        var userId = GetUserId();
        var conversations = await _messagingService.GetConversationsAsync(userId);
        return Ok(conversations);
    }

    /// <summary>
    /// Get messages in a conversation with a participant.
    /// </summary>
    [HttpGet("conversation/{participantId:guid}")]
    public async Task<ActionResult<List<MessageDto>>> GetConversation(Guid participantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var messages = await _messagingService.GetConversationAsync(userId, participantId, page, pageSize);
        return Ok(messages);
    }

    /// <summary>
    /// Mark a conversation as read.
    /// </summary>
    [HttpPost("conversation/{participantId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid participantId)
    {
        var userId = GetUserId();
        await _messagingService.MarkConversationAsReadAsync(userId, participantId);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
