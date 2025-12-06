using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class Refund : BaseEntity
{
    public Guid PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    
    public decimal Amount { get; set; }
    
    public Currency Currency { get; set; }
    
    public RefundStatus Status { get; set; }
    
    public string? Reason { get; set; }
    
    public string? ProviderRefundId { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public string? FailureReason { get; set; }
}

public enum RefundStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

