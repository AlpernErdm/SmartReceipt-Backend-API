using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid SubscriptionPlanId { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    
    public SubscriptionStatus Status { get; set; }
    
    public BillingPeriod BillingPeriod { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public DateTime? CancelledAt { get; set; }
    
    public string? CancellationReason { get; set; }
    
    public DateTime? NextBillingDate { get; set; }
    
    public bool AutoRenew { get; set; } = true;
    
    public string? PaymentProviderSubscriptionId { get; set; } // Stripe, iyzico vb. subscription ID
    
    // Navigation property
    public ICollection<UsageTracking> UsageTrackings { get; set; } = new List<UsageTracking>();
}

