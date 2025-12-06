using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<PaymentResult> CreatePaymentAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);
    
    Task<PaymentResult> ProcessPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
    
    Task<PaymentResult> GetPaymentStatusAsync(string paymentId, CancellationToken cancellationToken = default);
    
    Task<RefundResult> CreateRefundAsync(CreateRefundRequest request, CancellationToken cancellationToken = default);
    
    Task<bool> VerifyWebhookAsync(string signature, string payload, PaymentProvider provider);
}

public class CreatePaymentRequest
{
    public Guid UserId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public PaymentProvider Provider { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? PaymentId { get; set; }
    public string? TransactionId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public PaymentStatus Status { get; set; }
}

public class CreateRefundRequest
{
    public Guid PaymentId { get; set; }
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
}

public class RefundResult
{
    public bool IsSuccess { get; set; }
    public string? RefundId { get; set; }
    public string? ErrorMessage { get; set; }
    public RefundStatus Status { get; set; }
}

public enum RefundStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

