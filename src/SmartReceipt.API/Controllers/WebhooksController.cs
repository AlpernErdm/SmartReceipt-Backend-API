using Microsoft.AspNetCore.Mvc;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController : ControllerBase
{
    private readonly IWebhookService _webhookService;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        IWebhookService webhookService,
        ILogger<WebhooksController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    [HttpPost("iyzico")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> IyzicoWebhook([FromBody] object payload)
    {
        try
        {
            var signature = Request.Headers["X-Iyzico-Signature"].FirstOrDefault() ?? string.Empty;
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

            var request = new WebhookRequest
            {
                Provider = PaymentProvider.Iyzico,
                EventType = ExtractEventType(payload),
                Payload = payloadJson,
                Signature = signature,
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            await _webhookService.ProcessWebhookAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing iyzico webhook");
            return StatusCode(500);
        }
    }

    [HttpPost("stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StripeWebhook([FromBody] object payload)
    {
        try
        {
            var signature = Request.Headers["Stripe-Signature"].FirstOrDefault() ?? string.Empty;
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

            var request = new WebhookRequest
            {
                Provider = PaymentProvider.Stripe,
                EventType = ExtractEventType(payload),
                Payload = payloadJson,
                Signature = signature,
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            await _webhookService.ProcessWebhookAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing stripe webhook");
            return StatusCode(500);
        }
    }

    [HttpPost("paypal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PayPalWebhook([FromBody] object payload)
    {
        try
        {
            var signature = Request.Headers["Paypal-Transmission-Sig"].FirstOrDefault() ?? string.Empty;
            var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);

            var request = new WebhookRequest
            {
                Provider = PaymentProvider.PayPal,
                EventType = ExtractEventType(payload),
                Payload = payloadJson,
                Signature = signature,
                Headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
            };

            await _webhookService.ProcessWebhookAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing paypal webhook");
            return StatusCode(500);
        }
    }

    private string ExtractEventType(object payload)
    {
        return "payment.completed";
    }
}

