using AndyTipster.Application.Payments.Services;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripeService _stripeService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(IStripeService stripeService, ILogger<StripeWebhookController> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";

        var result = await _stripeService.ProcessWebhookAsync(payload, signature);

        if (!result.Success && result.ErrorMessage == "Invalid webhook signature.")
            return BadRequest(new { error = "Invalid webhook signature." });

        if (!result.Success)
        {
            _logger.LogError("Stripe webhook processing failed: {Error}", result.ErrorMessage);
            return StatusCode(500, new { error = result.ErrorMessage });
        }

        return Ok();
    }
}
