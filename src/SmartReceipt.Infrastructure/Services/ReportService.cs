using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _context;
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IApplicationDbContext context,
        IAnalyticsService analyticsService,
        ILogger<ReportService> logger)
    {
        _context = context;
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<byte[]> GeneratePdfReportAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating PDF report for user {UserId}, type: {Type}", 
            request.UserId, request.Type);

        var content = await GenerateReportContentAsync(request, cancellationToken);
        
        var pdfBytes = Encoding.UTF8.GetBytes(content);
        
        return pdfBytes;
    }

    public async Task<byte[]> GenerateExcelReportAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Excel report for user {UserId}, type: {Type}", 
            request.UserId, request.Type);

        var csvBytes = await GenerateCsvReportAsync(request, cancellationToken);
        
        return csvBytes;
    }

    public async Task<byte[]> GenerateCsvReportAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating CSV report for user {UserId}, type: {Type}", 
            request.UserId, request.Type);

        var sb = new StringBuilder();
        
        switch (request.Type)
        {
            case ReportType.Receipts:
                await GenerateReceiptsCsvAsync(request, sb, cancellationToken);
                break;
            case ReportType.CategoryAnalysis:
                await GenerateCategoryAnalysisCsvAsync(request, sb, cancellationToken);
                break;
            case ReportType.TaxReport:
                await GenerateTaxReportCsvAsync(request, sb, cancellationToken);
                break;
            default:
                sb.AppendLine("Report type not implemented");
                break;
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private async Task<string> GenerateReportContentAsync(ReportRequest request, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Report Type: {request.Type}");
        sb.AppendLine($"User ID: {request.UserId}");
        sb.AppendLine($"Period: {request.FromDate} - {request.ToDate}");
        sb.AppendLine();
        
        return sb.ToString();
    }

    private async Task GenerateReceiptsCsvAsync(ReportRequest request, StringBuilder sb, CancellationToken cancellationToken)
    {
        sb.AppendLine("Store Name,Date,Total Amount,Tax Amount,Items Count");
        
        var query = _context.Receipts
            .Include(r => r.Items)
            .Where(r => r.UserId == request.UserId);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.ReceiptDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(r => r.ReceiptDate <= request.ToDate.Value);

        var receipts = await query.ToListAsync(cancellationToken);

        foreach (var receipt in receipts)
        {
            sb.AppendLine($"{receipt.StoreName},{receipt.ReceiptDate:yyyy-MM-dd},{receipt.TotalAmount},{receipt.TaxAmount},{receipt.Items.Count}");
        }
    }

    private async Task GenerateCategoryAnalysisCsvAsync(ReportRequest request, StringBuilder sb, CancellationToken cancellationToken)
    {
        sb.AppendLine("Category,Amount,Receipt Count,Percentage");
        
        var analytics = await _analyticsService.GetCategoryAnalyticsAsync(
            request.UserId, 
            request.FromDate, 
            request.ToDate, 
            cancellationToken);

        foreach (var category in analytics.CategorySpendings)
        {
            sb.AppendLine($"{category.CategoryName},{category.Amount},{category.ReceiptCount},{category.Percentage:F2}%");
        }
    }

    private async Task GenerateTaxReportCsvAsync(ReportRequest request, StringBuilder sb, CancellationToken cancellationToken)
    {
        var year = request.Year ?? DateTime.UtcNow.Year;
        var month = request.Month;

        sb.AppendLine($"Tax Report - {year}" + (month.HasValue ? $" - {month}" : ""));
        sb.AppendLine();
        sb.AppendLine("Category,Amount,Tax Amount,Tax Rate");
        
        var taxReport = await _analyticsService.GetTaxReportAsync(
            request.UserId, 
            year, 
            month, 
            cancellationToken);

        foreach (var breakdown in taxReport.Breakdown)
        {
            sb.AppendLine($"{breakdown.Category},{breakdown.Amount},{breakdown.TaxAmount},{breakdown.TaxRate}%");
        }
        
        sb.AppendLine();
        sb.AppendLine($"Total Amount: {taxReport.TotalAmount}");
        sb.AppendLine($"Total Tax: {taxReport.TotalTaxAmount}");
        sb.AppendLine($"KDV: {taxReport.KdvAmount}");
        sb.AppendLine($"ÖTV: {taxReport.OtvAmount}");
    }
}

