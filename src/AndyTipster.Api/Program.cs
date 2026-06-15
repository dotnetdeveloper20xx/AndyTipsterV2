using System.Threading.RateLimiting;
using AndyTipster.Api.Endpoints;
using AndyTipster.Api.Middleware;
using AndyTipster.Application;
using AndyTipster.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.TelemetryConverters;

// Configure Serilog bootstrap logger for startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AndyTipster API");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog Structured Logging with Application Insights ───────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}");

        // Add Application Insights sink when connection string is configured
        var aiConnectionString = context.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrEmpty(aiConnectionString))
        {
            configuration.WriteTo.ApplicationInsights(aiConnectionString, TelemetryConverter.Traces);
        }
    });

    // ─── Application Insights Telemetry ─────────────────────────────────────────
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });

    // ─── Layer Dependency Injection ─────────────────────────────────────────────
    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // ─── Authentication & Authorization ─────────────────────────────────────────
    // JWT Bearer auth is configured in Infrastructure DependencyInjection
    // Authorization policies (permission-based) are also registered in Infrastructure DependencyInjection

    // ─── Controllers (for complex domain operations) ────────────────────────────
    builder.Services.AddControllers();

    // ─── ProblemDetails (RFC 7807) ──────────────────────────────────────────────
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        };
    });
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    // ─── CORS (allow Angular frontend on localhost:4200) ────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngularDev", policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                    ?? ["http://localhost:4200", "https://localhost:4200"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });

        options.AddPolicy("AllowProduction", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:ProductionOrigins").Get<string[]>();
            if (origins is { Length: > 0 })
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        });
    });

    // ─── Rate Limiting ──────────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Auth endpoints: 100 requests per minute per IP
        options.AddPolicy("AuthRateLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // General endpoints: 1000 requests per minute per authenticated user (or IP for anonymous)
        options.AddPolicy("GeneralRateLimit", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User?.Identity?.Name
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        // Global fallback limiter
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.User?.Identity?.Name
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));

        options.OnRejected = async (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.HttpContext.Response.ContentType = "application/problem+json";

            var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Type = "https://andytipster.com/errors/rate-limit-exceeded",
                Title = "Too Many Requests",
                Status = StatusCodes.Status429TooManyRequests,
                Detail = "Rate limit exceeded. Please try again later.",
                Instance = context.HttpContext.Request.Path
            };

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                problem.Extensions["retryAfter"] = (int)retryAfter.TotalSeconds;
            }

            await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        };
    });

    // ─── Health Checks ──────────────────────────────────────────────────────────
    var healthChecksBuilder = builder.Services.AddHealthChecks()
        .AddSqlServer(
            connectionString: builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=(localdb)\\mssqllocaldb;Database=AndyTipster;Trusted_Connection=True;",
            name: "database",
            tags: ["db", "sql", "ready"])
        .AddUrlGroup(
            uri: new Uri(builder.Configuration["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com/v1/"),
            name: "paypal-api",
            tags: ["external", "paypal", "ready"]);

    // Azure Blob Storage health check - only add if a real connection string is configured
    var storageConnectionString = builder.Configuration["AzureStorage:ConnectionString"];
    if (!string.IsNullOrEmpty(storageConnectionString) && storageConnectionString != "UseDevelopmentStorage=true")
    {
        healthChecksBuilder.AddAzureBlobStorage(
            clientFactory: _ => new Azure.Storage.Blobs.BlobServiceClient(storageConnectionString),
            name: "azure-blob-storage",
            tags: ["storage", "blob", "ready"]);
    }

    // ─── OpenAPI / Swagger ──────────────────────────────────────────────────────
    builder.Services.AddOpenApi();

    // ─── HTTPS Enforcement ──────────────────────────────────────────────────────
    if (!builder.Environment.IsDevelopment())
    {
        builder.Services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });
    }

    var app = builder.Build();

    // ─── Middleware Pipeline ────────────────────────────────────────────────────

    // Exception handling (first in pipeline to catch all errors)
    app.UseExceptionHandler();

    // HTTPS redirection
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }
    app.UseHttpsRedirection();

    // Security headers
    app.UseMiddleware<SecurityHeadersMiddleware>();

    // Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    // CORS
    app.UseCors(app.Environment.IsDevelopment() ? "AllowAngularDev" : "AllowProduction");

    // Rate limiting
    app.UseRateLimiter();

    // Routing
    app.UseRouting();

    // Auth
    app.UseAuthentication();
    app.UseAuthorization();

    // ─── Endpoint Mapping ───────────────────────────────────────────────────────

    // Controllers (complex domain operations)
    app.MapControllers()
        .RequireRateLimiting("GeneralRateLimit");

    // Minimal API endpoints (simple CRUD)
    app.MapStatusEndpoints();

    // Health check endpoints
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = WriteHealthCheckResponse
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = WriteHealthCheckResponse
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Liveness: just checks the app is running
    });

    // OpenAPI
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// ─── Health Check Response Writer ───────────────────────────────────────────────
static Task WriteHealthCheckResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        duration = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            duration = entry.Value.Duration.TotalMilliseconds,
            description = entry.Value.Description,
            exception = entry.Value.Exception?.Message,
            tags = entry.Value.Tags
        })
    };

    return context.Response.WriteAsJsonAsync(response);
}

// Required for integration testing with WebApplicationFactory
public partial class Program { }
