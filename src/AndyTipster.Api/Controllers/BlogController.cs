using System.Security.Claims;
using AndyTipster.Application.Blog.DTOs;
using AndyTipster.Application.Blog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    /// <summary>
    /// Get blog posts (admin view with all statuses).
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult> GetPosts([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _blogService.GetPostsAsync(status, page, pageSize);
        return Ok(new { items, totalCount });
    }

    /// <summary>
    /// Get published blog posts for public listing.
    /// </summary>
    [HttpGet("published")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPublishedPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _blogService.GetPostsAsync("Published", page, pageSize);
        return Ok(new { items, totalCount });
    }

    /// <summary>
    /// Get a blog post by slug (public).
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<BlogPostDto>> GetPostBySlug(string slug)
    {
        var post = await _blogService.GetPostBySlugAsync(slug);
        if (post == null) return NotFound();
        return Ok(post);
    }

    /// <summary>
    /// Get a blog post by ID (admin).
    /// </summary>
    [HttpGet("{postId:guid}")]
    [Authorize(Policy = "Permission:CMS.View")]
    public async Task<ActionResult<BlogPostDto>> GetPost(Guid postId)
    {
        var post = await _blogService.GetPostByIdAsync(postId);
        return Ok(post);
    }

    /// <summary>
    /// Create a new blog post.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Permission:CMS.Create")]
    public async Task<ActionResult<BlogPostDto>> CreatePost([FromBody] CreateBlogPostDto dto)
    {
        var userId = GetUserId();
        var post = await _blogService.CreatePostAsync(dto, userId);
        return CreatedAtAction(nameof(GetPost), new { postId = post.Id }, post);
    }

    /// <summary>
    /// Update a blog post.
    /// </summary>
    [HttpPatch("{postId:guid}")]
    [Authorize(Policy = "Permission:CMS.Edit")]
    public async Task<ActionResult<BlogPostDto>> UpdatePost(Guid postId, [FromBody] UpdateBlogPostDto dto)
    {
        var post = await _blogService.UpdatePostAsync(postId, dto);
        return Ok(post);
    }

    /// <summary>
    /// Delete a blog post.
    /// </summary>
    [HttpDelete("{postId:guid}")]
    [Authorize(Policy = "Permission:CMS.Delete")]
    public async Task<IActionResult> DeletePost(Guid postId)
    {
        await _blogService.DeletePostAsync(postId);
        return NoContent();
    }

    /// <summary>
    /// Publish a blog post (optionally schedule).
    /// </summary>
    [HttpPost("{postId:guid}/publish")]
    [Authorize(Policy = "Permission:CMS.Publish")]
    public async Task<ActionResult<BlogPostDto>> PublishPost(Guid postId, [FromBody] PublishBlogPostRequest? request)
    {
        var post = await _blogService.PublishPostAsync(postId, request?.ScheduledPublishAt);
        return Ok(post);
    }

    /// <summary>
    /// Unpublish a blog post (revert to draft).
    /// </summary>
    [HttpPost("{postId:guid}/unpublish")]
    [Authorize(Policy = "Permission:CMS.Publish")]
    public async Task<ActionResult<BlogPostDto>> UnpublishPost(Guid postId)
    {
        var post = await _blogService.UnpublishPostAsync(postId);
        return Ok(post);
    }

    private Guid GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
