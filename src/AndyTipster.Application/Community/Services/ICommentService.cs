using AndyTipster.Application.Community.DTOs;

namespace AndyTipster.Application.Community.Services;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(CreateCommentDto dto, Guid userId);
    Task<List<CommentDto>> GetCommentsForTipAsync(Guid tipId, int page = 1, int pageSize = 20);
    Task DeleteCommentAsync(Guid commentId);
    Task HideCommentAsync(Guid commentId);
}
