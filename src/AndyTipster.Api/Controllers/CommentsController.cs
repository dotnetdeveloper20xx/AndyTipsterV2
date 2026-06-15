using System.Security.Claims;
using AndyTipster.Application.Community.DTOs;
using AndyTipster.Application.Community.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// Get comments for a tip.
    /// </summary>
    [HttpGet("tip/{tipId:guid}")]
    [Authorize]
    public async Task<ActionResult<List<CommentDto>>> GetCommentsForTip(Guid tipId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var comments = await _commentService.GetCommentsForTipAsync(tipId, page, pageSize);
        return Ok(comments);
    }

    /// <summary>
    /// Post a comment on a tip.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentDto>> CreateComment([FromBody] CreateCommentDto dto)
    {
        var userId = GetUserId();
        var comment = await _commentService.CreateCommentAsync(dto, userId);
        return CreatedAtAction(nameof(GetCommentsForTip), new { tipId = dto.TipId }, comment);
    }

    /// <summary>
    /// Delete a comment (moderator action).
    /// </summary>
    [HttpDelete("{commentId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Moderator")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        await _commentService.DeleteCommentAsync(commentId);
        return NoContent();
    }

    /// <summary>
    /// Hide a comment (moderator action - soft delete).
    /// </summary>
    [HttpPost("{commentId:guid}/hide")]
    [Authorize(Roles = "Admin,SuperAdmin,Moderator")]
    public async Task<IActionResult> HideComment(Guid commentId)
    {
        await _commentService.HideCommentAsync(commentId);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
