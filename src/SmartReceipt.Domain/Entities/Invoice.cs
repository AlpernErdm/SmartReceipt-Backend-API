using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class Invoice : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public Currency Currency { get; set; } = Currency.TRY;
    
    public DateTime IssueDate { get; set; }
    
    public DateTime DueDate { get; set; }
    
    public bool IsPaid { get; set; }
    
    public DateTime? PaidAt { get; set; }
    
    public string? Description { get; set; }
    
    public string? BillingAddress { get; set; }
    
    public string? TaxNumber { get; set; }
    
    // Navigation properties
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

