using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IAnalyticsService
{
    Task<CategoryAnalyticsDto> GetCategoryAnalyticsAsync(Guid userId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    
    Task<TrendAnalyticsDto> GetTrendAnalyticsAsync(Guid userId, TrendPeriod period, CancellationToken cancellationToken = default);
    
    Task<StoreAnalyticsDto> GetStoreAnalyticsAsync(Guid userId, DateTime? fromDate, DateTime? toDate, int topCount = 10, CancellationToken cancellationToken = default);
    
    Task<TaxReportDto> GetTaxReportAsync(Guid userId, int year, int? month, CancellationToken cancellationToken = default);
    
    Task<ComparisonAnalyticsDto> GetComparisonAnalyticsAsync(Guid userId, ComparisonType type, CancellationToken cancellationToken = default);
}

public class CategoryAnalyticsDto
{
    public List<CategorySpending> CategorySpendings { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalReceipts { get; set; }
}

public class CategorySpending
{
    public ExpenseCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int ReceiptCount { get; set; }
    public decimal Percentage { get; set; }
}

public class TrendAnalyticsDto
{
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal? GrowthPercentage { get; set; }
}

public class TrendDataPoint
{
    public string Period { get; set; } = string.Empty; // "2024-01", "2024-02", etc.
    public decimal Amount { get; set; }
    public int ReceiptCount { get; set; }
}

public class StoreAnalyticsDto
{
    public List<StoreSpending> TopStores { get; set; } = new();
    public int TotalStores { get; set; }
}

public class StoreSpending
{
    public string StoreName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ReceiptCount { get; set; }
    public DateTime LastPurchaseDate { get; set; }
}

public class TaxReportDto
{
    public int Year { get; set; }
    public int? Month { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal KdvAmount { get; set; }
    public decimal OtvAmount { get; set; }
    public List<TaxBreakdown> Breakdown { get; set; } = new();
}

public class TaxBreakdown
{
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TaxRate { get; set; }
}

public class ComparisonAnalyticsDto
{
    public ComparisonType Type { get; set; }
    public List<ComparisonPeriod> Periods { get; set; } = new();
    public decimal? ChangePercentage { get; set; }
    public decimal? ChangeAmount { get; set; }
}

public class ComparisonPeriod
{
    public string PeriodLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int ReceiptCount { get; set; }
}

public enum TrendPeriod
{
    Monthly = 1,
    Yearly = 2
}

public enum ComparisonType
{
    MonthOverMonth = 1,
    YearOverYear = 2,
    Custom = 3
}

