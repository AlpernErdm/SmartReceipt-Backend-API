using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IReportService
{
    Task<byte[]> GeneratePdfReportAsync(ReportRequest request, CancellationToken cancellationToken = default);
    
    Task<byte[]> GenerateExcelReportAsync(ReportRequest request, CancellationToken cancellationToken = default);
    
    Task<byte[]> GenerateCsvReportAsync(ReportRequest request, CancellationToken cancellationToken = default);
}

public class ReportRequest
{
    public Guid UserId { get; set; }
    public ReportType Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public ExpenseCategory? Category { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
}

public enum ReportType
{
    Receipts = 1,
    CategoryAnalysis = 2,
    TaxReport = 3,
    TrendAnalysis = 4,
    StoreAnalysis = 5,
    BudgetReport = 6
}

