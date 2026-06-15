using AndyTipster.Application.Blog.DTOs;

namespace AndyTipster.Application.Blog.Services;

public interface IBlogService
{
    Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, Guid authorId);
    Task<BlogPostDto> UpdatePostAsync(Guid postId, UpdateBlogPostDto dto);
    Task<BlogPostDto> GetPostByIdAsync(Guid postId);
    Task<BlogPostDto?> GetPostBySlugAsync(string slug);
    Task<(List<BlogPostListItemDto> Items, int TotalCount)> GetPostsAsync(string? status, int page = 1, int pageSize = 10);
    Task DeletePostAsync(Guid postId);
    Task<BlogPostDto> PublishPostAsync(Guid postId, DateTime? scheduledPublishAt);
    Task<BlogPostDto> UnpublishPostAsync(Guid postId);
}
