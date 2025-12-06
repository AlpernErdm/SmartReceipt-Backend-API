using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Services;

public class WebhookService : IWebhookService
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IApplicationDbContext context,
        IPaymentService paymentService,
        ILogger<WebhookService> logger)
    {
        _context = context;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task ProcessWebhookAsync(WebhookRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing webhook from {Provider}, event: {EventType}", 
            request.Provider, request.EventType);

        var isValid = await VerifyWebhookSignatureAsync(
            request.Signature, 
            request.Payload, 
            request.Provider);

        if (!isValid)
        {
            _logger.LogWarning("Invalid webhook signature from {Provider}", request.Provider);
            throw new UnauthorizedAccessException("Invalid webhook signature");
        }

        switch (request.EventType.ToLower())
        {
            case "payment.completed":
            case "payment.succeeded":
                await HandlePaymentCompletedAsync(request, cancellationToken);
                break;
            case "payment.failed":
                await HandlePaymentFailedAsync(request, cancellationToken);
                break;
            case "payment.refunded":
                await HandlePaymentRefundedAsync(request, cancellationToken);
                break;
            case "subscription.cancelled":
                await HandleSubscriptionCancelledAsync(request, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unknown webhook event type: {EventType}", request.EventType);
                break;
        }
    }

    public async Task<bool> VerifyWebhookSignatureAsync(string signature, string payload, PaymentProvider provider)
    {
        return await _paymentService.VerifyWebhookAsync(signature, payload, provider);
    }

    private async Task HandlePaymentCompletedAsync(WebhookRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment completed webhook processed");
    }

    private async Task HandlePaymentFailedAsync(WebhookRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment failed webhook processed");
    }

    private async Task HandlePaymentRefundedAsync(WebhookRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment refunded webhook processed");
    }

    private async Task HandleSubscriptionCancelledAsync(WebhookRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Subscription cancelled webhook processed");
    }
}

