namespace AndyTipster.Api.Endpoints;

/// <summary>
/// Minimal API endpoints for simple operations (status, info).
/// Demonstrates the Minimal APIs pattern for lightweight CRUD endpoints.
/// </summary>
public static class StatusEndpoints
{
    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/status")
            .WithTags("Status");

        group.MapGet("/", () => Results.Ok(new
        {
            Status = "Healthy",
            Version = "2.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Timestamp = DateTime.UtcNow
        }))
        .WithName("GetStatus")
        .WithDescription("Returns the current API status and version information")
        .Produces(StatusCodes.Status200OK);

        return routes;
    }
}
