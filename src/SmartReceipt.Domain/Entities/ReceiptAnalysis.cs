using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class ReceiptAnalysis : BaseEntity
{
    public Guid ReceiptId { get; set; }
    public Receipt Receipt { get; set; } = null!;
    
    public decimal TotalByCategory { get; set; }
    
    public ExpenseCategory PrimaryCategory { get; set; }
    
    public string? CategoryBreakdownJson { get; set; } // JSON string for Dictionary<ExpenseCategory, decimal>
    
    public bool IsRecurring { get; set; }
    
    public Guid? RecurringExpenseGroupId { get; set; }
    
    public decimal? AverageAmount { get; set; }
    
    public int? FrequencyDays { get; set; }
    
    public bool IsVerified { get; set; }
    
    public decimal VerificationScore { get; set; } // 0-100
    
    public bool IsFraudulent { get; set; }
    
    public string? FraudReason { get; set; }
    
    public string? SuggestedCategory { get; set; } // ML suggestion
}

