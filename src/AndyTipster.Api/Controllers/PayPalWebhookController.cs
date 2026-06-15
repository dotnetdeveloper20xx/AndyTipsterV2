using AndyTipster.Application.Payments.Services;
using Microsoft.AspNetCore.Mvc;

namespace AndyTipster.Api.Controllers;

[ApiController]
[Route("api/webhooks/paypal")]
public class PayPalWebhookController : ControllerBase
{
    private readonly IPayPalService _paypalService;
    private readonly ILogger<PayPalWebhookController> _logger;

    public PayPalWebhookController(IPayPalService paypalService, ILogger<PayPalWebhookController> logger)
    {
        _paypalService = paypalService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync();

        var signature = Request.Headers["PAYPAL-TRANSMISSION-SIG"].FirstOrDefault() ?? "";

        var result = await _paypalService.ProcessWebhookAsync(payload, signature);

        if (!result.Success && result.ErrorMessage == "Invalid webhook signature.")
            return Unauthorized(new { error = "Invalid webhook signature." });

        if (!result.Success)
        {
            _logger.LogError("PayPal webhook processing failed: {Error}", result.ErrorMessage);
            return StatusCode(500, new { error = result.ErrorMessage });
        }

        return Ok();
    }
}
