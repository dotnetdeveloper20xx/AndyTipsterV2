using AndyTipster.Application.Community.DTOs;
using AndyTipster.Application.Community.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Exceptions;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly AndyTipsterDbContext _context;

    public CommentService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    public async Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["content"] = new[] { "Comment content is required." }
            });

        var tip = await _context.Tips.FindAsync(dto.TipId)
            ?? throw new NotFoundException("Tip not found.");

        var user = await _context.Users.FindAsync(userId)
            ?? throw new NotFoundException("User not found.");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TipId = dto.TipId,
            Content = dto.Content.Trim(),
            IsApproved = true, // Auto-approve for now
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            AuthorName = user.DisplayName ?? user.UserName ?? "",
            AuthorAvatarUrl = user.AvatarUrl,
            TipId = comment.TipId,
            Content = comment.Content,
            IsApproved = comment.IsApproved,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<List<CommentDto>> GetCommentsForTipAsync(Guid tipId, int page = 1, int pageSize = 20)
    {
        return await _context.Comments
            .Include(c => c.User)
            .Where(c => c.TipId == tipId && c.IsApproved)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                UserId = c.UserId,
                AuthorName = c.User.DisplayName ?? c.User.UserName ?? "",
                AuthorAvatarUrl = c.User.AvatarUrl,
                TipId = c.TipId,
                Content = c.Content,
                IsApproved = c.IsApproved,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task DeleteCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId)
            ?? throw new NotFoundException("Comment not found.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    public async Task HideCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId)
            ?? throw new NotFoundException("Comment not found.");

        comment.IsApproved = false;
        await _context.SaveChangesAsync();
    }
}
