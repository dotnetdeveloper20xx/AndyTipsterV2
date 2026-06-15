using AndyTipster.Application.Blog.DTOs;
using AndyTipster.Application.Blog.Services;
using AndyTipster.Domain.Entities;
using AndyTipster.Domain.Enumerations;
using AndyTipster.Domain.Exceptions;
using AndyTipster.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AndyTipster.Infrastructure.Services;

public class BlogService : IBlogService
{
    private readonly AndyTipsterDbContext _context;

    public BlogService(AndyTipsterDbContext context)
    {
        _context = context;
    }

    public async Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto, Guid authorId)
    {
        var errors = ValidatePost(dto.Title, dto.Content);
        if (errors.Count > 0)
            throw new ValidationException(errors);

        var slug = GenerateSlug(dto.Title);

        // Ensure unique slug
        var slugBase = slug;
        var counter = 1;
        while (await _context.BlogPosts.AnyAsync(p => p.Slug == slug))
        {
            slug = $"{slugBase}-{counter}";
            counter++;
        }

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Slug = slug,
            Content = dto.Content,
            Excerpt = dto.Excerpt?.Trim(),
            FeaturedImageUrl = dto.FeaturedImageUrl,
            MetaTitle = dto.MetaTitle?.Trim(),
            MetaDescription = dto.MetaDescription?.Trim(),
            Status = PageStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            AuthorId = authorId
        };

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(post);
    }

    public async Task<BlogPostDto> UpdatePostAsync(Guid postId, UpdateBlogPostDto dto)
    {
        var post = await _context.BlogPosts.FindAsync(postId)
            ?? throw new NotFoundException("Blog post not found.");

        if (dto.Title != null)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["title"] = new[] { "Title is required." }
                });
            post.Title = dto.Title.Trim();
            post.Slug = GenerateSlug(dto.Title);
        }

        if (dto.Content != null)
            post.Content = dto.Content;

        if (dto.Excerpt != null)
            post.Excerpt = dto.Excerpt.Trim();

        if (dto.FeaturedImageUrl != null)
            post.FeaturedImageUrl = dto.FeaturedImageUrl;

        if (dto.MetaTitle != null)
            post.MetaTitle = dto.MetaTitle.Trim();

        if (dto.MetaDescription != null)
            post.MetaDescription = dto.MetaDescription.Trim();

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(post);
    }

    public async Task<BlogPostDto> GetPostByIdAsync(Guid postId)
    {
        var post = await _context.BlogPosts.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == postId)
            ?? throw new NotFoundException("Blog post not found.");
        return MapToDto(post);
    }

    public async Task<BlogPostDto?> GetPostBySlugAsync(string slug)
    {
        var post = await _context.BlogPosts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == PageStatus.Published);

        return post != null ? MapToDto(post) : null;
    }

    public async Task<(List<BlogPostListItemDto> Items, int TotalCount)> GetPostsAsync(string? status, int page = 1, int pageSize = 10)
    {
        var query = _context.BlogPosts.Include(p => p.Author).AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PageStatus>(status, true, out var pageStatus))
            query = query.Where(p => p.Status == pageStatus);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new BlogPostListItemDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Excerpt = p.Excerpt,
                FeaturedImageUrl = p.FeaturedImageUrl,
                Status = p.Status.ToString(),
                PublishedAt = p.PublishedAt,
                AuthorName = p.Author.DisplayName ?? p.Author.UserName ?? ""
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task DeletePostAsync(Guid postId)
    {
        var post = await _context.BlogPosts.FindAsync(postId)
            ?? throw new NotFoundException("Blog post not found.");

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();
    }

    public async Task<BlogPostDto> PublishPostAsync(Guid postId, DateTime? scheduledPublishAt)
    {
        var post = await _context.BlogPosts.FindAsync(postId)
            ?? throw new NotFoundException("Blog post not found.");

        if (post.Status == PageStatus.Published)
            throw new BusinessRuleException("Blog post is already published.");

        if (scheduledPublishAt.HasValue)
        {
            if (scheduledPublishAt.Value <= DateTime.UtcNow.AddMinutes(1))
                throw new BusinessRuleException("Scheduled publish time must be at least 1 minute in the future.");

            post.ScheduledPublishAt = scheduledPublishAt.Value;
            post.Status = PageStatus.Scheduled;
        }
        else
        {
            post.Status = PageStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(post);
    }

    public async Task<BlogPostDto> UnpublishPostAsync(Guid postId)
    {
        var post = await _context.BlogPosts.FindAsync(postId)
            ?? throw new NotFoundException("Blog post not found.");

        post.Status = PageStatus.Draft;
        post.PublishedAt = null;
        post.ScheduledPublishAt = null;

        await _context.SaveChangesAsync();
        return await MapToDtoAsync(post);
    }

    private static Dictionary<string, string[]> ValidatePost(string title, string content)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
            errors["title"] = new[] { "Title is required." };

        if (string.IsNullOrWhiteSpace(content))
            errors["content"] = new[] { "Content is required." };

        return errors;
    }

    private static string GenerateSlug(string title)
    {
        return title.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "");
    }

    private async Task<BlogPostDto> MapToDtoAsync(BlogPost post)
    {
        var author = await _context.Users.FindAsync(post.AuthorId);
        return new BlogPostDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Excerpt = post.Excerpt,
            FeaturedImageUrl = post.FeaturedImageUrl,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt,
            PublishedAt = post.PublishedAt,
            ScheduledPublishAt = post.ScheduledPublishAt,
            AuthorId = post.AuthorId,
            AuthorName = author?.DisplayName ?? author?.UserName ?? ""
        };
    }

    private static BlogPostDto MapToDto(BlogPost post)
    {
        return new BlogPostDto
        {
            Id = post.Id,
            Title = post.Title,
            Slug = post.Slug,
            Content = post.Content,
            Excerpt = post.Excerpt,
            FeaturedImageUrl = post.FeaturedImageUrl,
            MetaTitle = post.MetaTitle,
            MetaDescription = post.MetaDescription,
            Status = post.Status.ToString(),
            CreatedAt = post.CreatedAt,
            PublishedAt = post.PublishedAt,
            ScheduledPublishAt = post.ScheduledPublishAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author?.DisplayName ?? post.Author?.UserName ?? ""
        };
    }
}
