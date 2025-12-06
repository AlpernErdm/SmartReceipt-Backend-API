using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IWebhookService
{
    Task ProcessWebhookAsync(WebhookRequest request, CancellationToken cancellationToken = default);
    
    Task<bool> VerifyWebhookSignatureAsync(string signature, string payload, PaymentProvider provider);
}

public class WebhookRequest
{
    public PaymentProvider Provider { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
}

