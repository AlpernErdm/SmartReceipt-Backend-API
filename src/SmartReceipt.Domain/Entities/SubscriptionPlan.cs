using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public PlanType PlanType { get; set; }
    
    public decimal MonthlyPrice { get; set; }
    
    public decimal YearlyPrice { get; set; }
    
    public int MonthlyScanLimit { get; set; } // Aylık fiş tarama limiti
    
    public long StorageLimitMB { get; set; } // Depolama limiti (MB)
    
    public bool IsActive { get; set; } = true;
    
    public int? TrialDays { get; set; } // Trial gün sayısı (null ise trial yok)
    
    public bool HasApiAccess { get; set; } = false;
    
    public bool HasAdvancedAnalytics { get; set; } = false;
    
    public bool HasTeamManagement { get; set; } = false;
    
    public bool HasPrioritySupport { get; set; } = false;
    
    // Navigation property
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

