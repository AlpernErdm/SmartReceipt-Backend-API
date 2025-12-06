using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Domain.Entities;

public class Budget : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    
    public Currency Currency { get; set; } = Currency.TRY;
    
    public BudgetPeriod Period { get; set; }
    
    public int Year { get; set; }
    
    public int? Month { get; set; } // Null if yearly
    
    public ExpenseCategory? Category { get; set; } // Null if all categories
    
    public bool IsActive { get; set; } = true;
    
    public decimal? AlertThreshold { get; set; } // Percentage (e.g., 80 = alert at 80%)
}

public enum BudgetPeriod
{
    Monthly = 1,
    Yearly = 2
}

