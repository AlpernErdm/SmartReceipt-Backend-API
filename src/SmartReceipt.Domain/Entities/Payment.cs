using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    
    public PaymentProvider Provider { get; set; }
    
    public PaymentStatus Status { get; set; }
    
    public decimal Amount { get; set; }
    
    public Currency Currency { get; set; } = Currency.TRY;
    
    public string? ProviderTransactionId { get; set; }
    
    public string? ProviderPaymentId { get; set; }
    
    public string? PaymentMethod { get; set; } // Credit Card, Bank Transfer, etc.
    
    public string? FailureReason { get; set; }
    
    public DateTime? PaidAt { get; set; }
    
    public string? Metadata { get; set; } // JSON string for additional data
    
    // Navigation property
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}

