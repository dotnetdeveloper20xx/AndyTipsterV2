using System.Security.Claims;
using AndyTipster.Application.HelpBot.DTOs;
using AndyTipster.Application.HelpBot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/helpbot")]
public class HelpBotController : ControllerBase
{
    private readonly IHelpBotService _helpBotService;

    public HelpBotController(IHelpBotService helpBotService)
    {
        _helpBotService = helpBotService;
    }

    /// <summary>
    /// Send a message to the help bot and get a response.
    /// </summary>
    [HttpPost("message")]
    [AllowAnonymous]
    public async Task<ActionResult<BotResponseDto>> SendMessage([FromBody] UserMessageDto dto)
    {
        var userId = GetUserIdOrNull();
        var response = await _helpBotService.ProcessMessageAsync(dto, userId);
        return Ok(response);
    }

    /// <summary>
    /// Get conversation history by session ID.
    /// </summary>
    [HttpGet("conversation/{sessionId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid sessionId)
    {
        var conversation = await _helpBotService.GetConversationAsync(sessionId);
        return Ok(conversation);
    }

    /// <summary>
    /// Get all conversation flows (admin).
    /// </summary>
    [HttpGet("flows")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<List<FlowDto>>> GetFlows()
    {
        var flows = await _helpBotService.GetFlowsAsync();
        return Ok(flows);
    }

    /// <summary>
    /// Create a new conversation flow (admin).
    /// </summary>
    [HttpPost("flows")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<FlowDto>> CreateFlow([FromBody] CreateFlowDto dto)
    {
        var flow = await _helpBotService.CreateFlowAsync(dto);
        return CreatedAtAction(nameof(GetFlows), flow);
    }

    /// <summary>
    /// Update a conversation flow (admin).
    /// </summary>
    [HttpPatch("flows/{flowId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<FlowDto>> UpdateFlow(Guid flowId, [FromBody] UpdateFlowDto dto)
    {
        var flow = await _helpBotService.UpdateFlowAsync(flowId, dto);
        return Ok(flow);
    }

    /// <summary>
    /// Delete a conversation flow (admin).
    /// </summary>
    [HttpDelete("flows/{flowId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteFlow(Guid flowId)
    {
        await _helpBotService.DeleteFlowAsync(flowId);
        return NoContent();
    }

    /// <summary>
    /// Escalate a conversation to a support ticket.
    /// </summary>
    [HttpPost("conversation/{sessionId:guid}/escalate")]
    [AllowAnonymous]
    public async Task<IActionResult> Escalate(Guid sessionId, [FromBody] EscalateRequest request)
    {
        await _helpBotService.EscalateToTicketAsync(sessionId, request.Summary);
        return Ok(new { message = "Escalated to support ticket." });
    }

    private Guid? GetUserIdOrNull()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId != null ? Guid.Parse(userId) : null;
    }
}

public record EscalateRequest
{
    public string Summary { get; init; } = string.Empty;
}
