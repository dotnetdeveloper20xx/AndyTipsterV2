using System.Security.Claims;
using AndyTipster.Application.Community.DTOs;
using AndyTipster.Application.Community.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/polls")]
[Authorize]
public class PollsController : ControllerBase
{
    private readonly IPollService _pollService;

    public PollsController(IPollService pollService)
    {
        _pollService = pollService;
    }

    /// <summary>
    /// Get active polls.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PollDto>>> GetActivePolls()
    {
        var userId = GetUserId();
        var polls = await _pollService.GetActivePollsAsync(userId);
        return Ok(polls);
    }

    /// <summary>
    /// Get a specific poll.
    /// </summary>
    [HttpGet("{pollId:guid}")]
    public async Task<ActionResult<PollDto>> GetPoll(Guid pollId)
    {
        var userId = GetUserId();
        var poll = await _pollService.GetPollAsync(pollId, userId);
        return Ok(poll);
    }

    /// <summary>
    /// Create a new poll (admin only).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<PollDto>> CreatePoll([FromBody] CreatePollDto dto)
    {
        var userId = GetUserId();
        var poll = await _pollService.CreatePollAsync(dto, userId);
        return CreatedAtAction(nameof(GetPoll), new { pollId = poll.Id }, poll);
    }

    /// <summary>
    /// Vote on a poll.
    /// </summary>
    [HttpPost("{pollId:guid}/vote")]
    public async Task<ActionResult<PollDto>> Vote(Guid pollId, [FromBody] VoteDto dto)
    {
        var userId = GetUserId();
        var poll = await _pollService.VoteAsync(new VoteDto { PollId = pollId, OptionId = dto.OptionId }, userId);
        return Ok(poll);
    }

    /// <summary>
    /// Close a poll (admin only).
    /// </summary>
    [HttpPost("{pollId:guid}/close")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ClosePoll(Guid pollId)
    {
        await _pollService.ClosePollAsync(pollId);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
