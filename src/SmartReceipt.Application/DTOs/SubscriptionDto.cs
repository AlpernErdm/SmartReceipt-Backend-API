using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.DTOs;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int MonthlyScanLimit { get; set; }
    public long StorageLimitMB { get; set; }
    public int? TrialDays { get; set; }
    public bool HasApiAccess { get; set; }
    public bool HasAdvancedAnalytics { get; set; }
    public bool HasTeamManagement { get; set; }
    public bool HasPrioritySupport { get; set; }
}

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public SubscriptionPlanDto Plan { get; set; } = null!;
    public SubscriptionStatus Status { get; set; }
    public BillingPeriod BillingPeriod { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? NextBillingDate { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UsageDto
{
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public int ScanCount { get; set; }
    public int ScanLimit { get; set; }
    public long StorageUsedBytes { get; set; }
    public long StorageLimitBytes { get; set; }
    public int ApiCallCount { get; set; }
    public decimal UsagePercentage { get; set; }
    public bool IsLimitExceeded { get; set; }
}

public class CreateSubscriptionDto
{
    public Guid PlanId { get; set; }
    public BillingPeriod BillingPeriod { get; set; }
}

public class CancelSubscriptionDto
{
    public string? Reason { get; set; }
}

