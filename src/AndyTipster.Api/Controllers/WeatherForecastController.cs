using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

/// <summary>
/// Sample controller demonstrating the Controllers pattern for complex domain operations.
/// This will be replaced with real controllers in subsequent tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gets weather forecasts. Demonstrates controller-based endpoint pattern.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<WeatherForecast>>(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        _logger.LogInformation("Fetching weather forecasts");

        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Ok(forecasts);
    }
}

public record WeatherForecast
{
    public DateOnly Date { get; init; }
    public int TemperatureC { get; init; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; init; }
}
