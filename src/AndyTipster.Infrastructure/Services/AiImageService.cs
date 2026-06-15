namespace AndyTipster.Infrastructure.Services;

/// <summary>
/// AI Image Generation service integrating DALL-E and stock photo providers (Unsplash, Pexels).
/// </summary>
public interface IAiImageService
{
    Task<GeneratedImageResult> GenerateImageAsync(string prompt, string size = "1024x1024");
    Task<List<StockPhotoResult>> SearchUnsplashAsync(string query, int page = 1, int perPage = 20);
    Task<List<StockPhotoResult>> SearchPexelsAsync(string query, int page = 1, int perPage = 20);
}

public class AiImageService : IAiImageService
{
    private readonly HttpClient _httpClient;

    public AiImageService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("AiImage");
    }

    /// <summary>
    /// Generate an image using DALL-E API.
    /// This is a stub implementation - configure DALL-E API key in production.
    /// </summary>
    public async Task<GeneratedImageResult> GenerateImageAsync(string prompt, string size = "1024x1024")
    {
        // DALL-E API integration stub
        // In production: POST https://api.openai.com/v1/images/generations
        // with model: "dall-e-3", prompt, size, quality, n
        await Task.CompletedTask;

        return new GeneratedImageResult
        {
            Success = false,
            ErrorMessage = "DALL-E API key not configured. Set 'AiImage:DallE:ApiKey' in configuration.",
            ImageUrl = null,
            RevisedPrompt = null
        };
    }

    /// <summary>
    /// Search Unsplash for stock photos.
    /// This is a stub implementation - configure Unsplash Access Key in production.
    /// </summary>
    public async Task<List<StockPhotoResult>> SearchUnsplashAsync(string query, int page = 1, int perPage = 20)
    {
        // Unsplash API integration stub
        // In production: GET https://api.unsplash.com/search/photos?query={query}&page={page}&per_page={perPage}
        // with Authorization: Client-ID {access_key}
        await Task.CompletedTask;

        return new List<StockPhotoResult>
        {
            new()
            {
                Provider = "Unsplash",
                Id = "stub-unsplash-1",
                Description = "Unsplash API not configured. Set 'AiImage:Unsplash:AccessKey' in configuration.",
                ThumbnailUrl = "",
                FullUrl = "",
                Author = "N/A",
                AuthorUrl = ""
            }
        };
    }

    /// <summary>
    /// Search Pexels for stock photos.
    /// This is a stub implementation - configure Pexels API Key in production.
    /// </summary>
    public async Task<List<StockPhotoResult>> SearchPexelsAsync(string query, int page = 1, int perPage = 20)
    {
        // Pexels API integration stub
        // In production: GET https://api.pexels.com/v1/search?query={query}&page={page}&per_page={perPage}
        // with Authorization: {api_key}
        await Task.CompletedTask;

        return new List<StockPhotoResult>
        {
            new()
            {
                Provider = "Pexels",
                Id = "stub-pexels-1",
                Description = "Pexels API not configured. Set 'AiImage:Pexels:ApiKey' in configuration.",
                ThumbnailUrl = "",
                FullUrl = "",
                Author = "N/A",
                AuthorUrl = ""
            }
        };
    }
}

public class GeneratedImageResult
{
    public bool Success { get; set; }
    public string? ImageUrl { get; set; }
    public string? RevisedPrompt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class StockPhotoResult
{
    public string Provider { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string FullUrl { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string AuthorUrl { get; set; } = string.Empty;
}
