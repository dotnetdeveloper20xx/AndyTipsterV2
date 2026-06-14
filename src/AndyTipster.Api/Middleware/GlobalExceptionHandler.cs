using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Middleware;

/// <summary>
/// Global exception handler that returns RFC 7807 ProblemDetails responses.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            ArgumentException argEx => new ProblemDetails
            {
                Type = "https://andytipster.com/errors/bad-request",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
                Detail = argEx.Message,
                Instance = httpContext.Request.Path
            },
            UnauthorizedAccessException => new ProblemDetails
            {
                Type = "https://andytipster.com/errors/unauthorized",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Authentication is required to access this resource.",
                Instance = httpContext.Request.Path
            },
            KeyNotFoundException => new ProblemDetails
            {
                Type = "https://andytipster.com/errors/not-found",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = "The requested resource was not found.",
                Instance = httpContext.Request.Path
            },
            _ => new ProblemDetails
            {
                Type = "https://andytipster.com/errors/internal-server-error",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.",
                Instance = httpContext.Request.Path
            }
        };

        // Add trace ID for correlation
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
