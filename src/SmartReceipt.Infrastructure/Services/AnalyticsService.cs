using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IApplicationDbContext context,
        ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CategoryAnalyticsDto> GetCategoryAnalyticsAsync(
        Guid userId, 
        DateTime? fromDate, 
        DateTime? toDate, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Receipts
            .Include(r => r.Items)
            .Where(r => r.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(r => r.ReceiptDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.ReceiptDate <= toDate.Value);

        var receipts = await query.ToListAsync(cancellationToken);

        _logger.LogInformation("Category analytics: Found {ReceiptCount} receipts for user {UserId}", 
            receipts.Count, userId);

        var allItems = receipts.SelectMany(r => r.Items).ToList();
        
        _logger.LogInformation("Category analytics: Found {ItemCount} items total", allItems.Count);

        if (!allItems.Any())
        {
            _logger.LogWarning("No receipt items found for category analytics");
            return new CategoryAnalyticsDto
            {
                CategorySpendings = new List<CategorySpending>(),
                TotalAmount = 0,
                TotalReceipts = receipts.Count
            };
        }

        var categoryGroups = allItems
            .Where(item => !string.IsNullOrWhiteSpace(item.Category)) // Boş kategori'leri filtrele
            .GroupBy(item => item.Category)
            .Select(g => new CategorySpending
            {
                Category = Enum.TryParse<ExpenseCategory>(g.Key, true, out var category) ? category : ExpenseCategory.Other,
                CategoryName = g.Key,
                Amount = g.Sum(item => item.TotalPrice),
                ReceiptCount = g.Select(item => item.ReceiptId).Distinct().Count(),
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        _logger.LogInformation("Category analytics: Found {CategoryCount} categories", categoryGroups.Count);

        var totalAmount = categoryGroups.Sum(c => c.Amount);
        
        foreach (var category in categoryGroups)
        {
            category.Percentage = totalAmount > 0 ? (category.Amount / totalAmount) * 100 : 0;
        }

        return new CategoryAnalyticsDto
        {
            CategorySpendings = categoryGroups,
            TotalAmount = totalAmount,
            TotalReceipts = receipts.Count
        };
    }

    public async Task<TrendAnalyticsDto> GetTrendAnalyticsAsync(
        Guid userId, 
        TrendPeriod period, 
        CancellationToken cancellationToken = default)
    {
        List<TrendDataPoint> dataPoints = new();

        if (period == TrendPeriod.Monthly)
        {
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .Reverse();

            var dataPointsList = new List<TrendDataPoint>();

            foreach (var month in last12Months)
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var receipts = await _context.Receipts
                    .Where(r => r.UserId == userId && r.ReceiptDate >= monthStart && r.ReceiptDate <= monthEnd)
                    .ToListAsync(cancellationToken);

                dataPointsList.Add(new TrendDataPoint
                {
                    Period = monthStart.ToString("yyyy-MM"),
                    Amount = receipts.Sum(r => r.TotalAmount),
                    ReceiptCount = receipts.Count
                });
            }
            
            dataPoints = dataPointsList;
        }
        else // Yearly
        {
            var last5Years = Enumerable.Range(0, 5)
                .Select(i => DateTime.UtcNow.Year - i)
                .Reverse();

            var dataPointsList = new List<TrendDataPoint>();

            foreach (var year in last5Years)
            {
                var yearStart = new DateTime(year, 1, 1);
                var yearEnd = new DateTime(year, 12, 31);

                var receipts = await _context.Receipts
                    .Where(r => r.UserId == userId && r.ReceiptDate >= yearStart && r.ReceiptDate <= yearEnd)
                    .ToListAsync(cancellationToken);

                dataPointsList.Add(new TrendDataPoint
                {
                    Period = year.ToString(),
                    Amount = receipts.Sum(r => r.TotalAmount),
                    ReceiptCount = receipts.Count
                });
            }
            
            dataPoints = dataPointsList;
        }

        var totalAmount = dataPoints.Sum(d => d.Amount);
        var averageAmount = dataPoints.Any() ? totalAmount / dataPoints.Count : 0;

        decimal? growthPercentage = null;
        if (dataPoints.Count >= 2)
        {
            var first = dataPoints.First().Amount;
            var last = dataPoints.Last().Amount;
            if (first > 0)
            {
                growthPercentage = ((last - first) / first) * 100;
            }
        }

        return new TrendAnalyticsDto
        {
            DataPoints = dataPoints,
            TotalAmount = totalAmount,
            AverageAmount = averageAmount,
            GrowthPercentage = growthPercentage
        };
    }

    public async Task<StoreAnalyticsDto> GetStoreAnalyticsAsync(
        Guid userId, 
        DateTime? fromDate, 
        DateTime? toDate, 
        int topCount = 10, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Receipts
            .Where(r => r.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(r => r.ReceiptDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(r => r.ReceiptDate <= toDate.Value);

        var allReceipts = await query.ToListAsync(cancellationToken);
        
        _logger.LogInformation("Store analytics: Found {ReceiptCount} receipts for user {UserId}", 
            allReceipts.Count, userId);

        if (!allReceipts.Any())
        {
            _logger.LogWarning("No receipts found for store analytics");
            return new StoreAnalyticsDto
            {
                TopStores = new List<StoreSpending>(),
                TotalStores = 0
            };
        }

        var storeGroups = allReceipts
            .Where(r => !string.IsNullOrWhiteSpace(r.StoreName))
            .GroupBy(r => r.StoreName)
            .Select(g => new StoreSpending
            {
                StoreName = g.Key,
                TotalAmount = g.Sum(r => r.TotalAmount),
                ReceiptCount = g.Count(),
                LastPurchaseDate = g.Max(r => r.ReceiptDate)
            })
            .OrderByDescending(s => s.TotalAmount)
            .Take(topCount)
            .ToList();

        _logger.LogInformation("Store analytics: Found {StoreCount} stores", storeGroups.Count);

        var totalStores = allReceipts
            .Where(r => !string.IsNullOrWhiteSpace(r.StoreName))
            .Select(r => r.StoreName)
            .Distinct()
            .Count();

        return new StoreAnalyticsDto
        {
            TopStores = storeGroups,
            TotalStores = totalStores
        };
    }

    public async Task<TaxReportDto> GetTaxReportAsync(
        Guid userId, 
        int year, 
        int? month, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Receipts
            .Include(r => r.Items)
            .Where(r => r.UserId == userId && r.ReceiptDate.Year == year);

        if (month.HasValue)
            query = query.Where(r => r.ReceiptDate.Month == month.Value);

        var receipts = await query.ToListAsync(cancellationToken);

        var totalAmount = receipts.Sum(r => r.TotalAmount);
        var totalTaxAmount = receipts.Sum(r => r.TaxAmount);

        var kdvAmount = totalTaxAmount * 0.18m;
        var otvAmount = totalTaxAmount * 0.02m;

        var breakdown = receipts
            .SelectMany(r => r.Items)
            .GroupBy(item => item.Category)
            .Select(g => new TaxBreakdown
            {
                Category = Enum.TryParse<ExpenseCategory>(g.Key, out var category) ? category : ExpenseCategory.Other,
                Amount = g.Sum(item => item.TotalPrice),
                TaxAmount = g.Sum(item => item.TotalPrice) * 0.20m, // Örnek %20 vergi
                TaxRate = 20m
            })
            .ToList();

        return new TaxReportDto
        {
            Year = year,
            Month = month,
            TotalAmount = totalAmount,
            TotalTaxAmount = totalTaxAmount,
            KdvAmount = kdvAmount,
            OtvAmount = otvAmount,
            Breakdown = breakdown
        };
    }

    public async Task<ComparisonAnalyticsDto> GetComparisonAnalyticsAsync(
        Guid userId, 
        ComparisonType type, 
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        List<ComparisonPeriod> periods;

        if (type == ComparisonType.MonthOverMonth)
        {
            var currentMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = currentMonth.AddMonths(-1);

            periods = await GetPeriodComparison(userId, currentMonth, lastMonth, cancellationToken);
        }
        else if (type == ComparisonType.YearOverYear)
        {
            var currentYear = new DateTime(now.Year, 1, 1);
            var lastYear = currentYear.AddYears(-1);

            periods = await GetPeriodComparison(userId, currentYear, lastYear, cancellationToken);
        }
        else
        {
            periods = new List<ComparisonPeriod>();
        }

        decimal? changePercentage = null;
        decimal? changeAmount = null;

        if (periods.Count >= 2)
        {
            var first = periods[0].Amount;
            var second = periods[1].Amount;
            changeAmount = second - first;
            if (first > 0)
            {
                changePercentage = ((second - first) / first) * 100;
            }
        }

        return new ComparisonAnalyticsDto
        {
            Type = type,
            Periods = periods,
            ChangePercentage = changePercentage,
            ChangeAmount = changeAmount
        };
    }

    private async Task<List<ComparisonPeriod>> GetPeriodComparison(
        Guid userId,
        DateTime period1Start,
        DateTime period2Start,
        CancellationToken cancellationToken)
    {
        var period1End = period1Start.AddMonths(1).AddDays(-1);
        var period2End = period2Start.AddMonths(1).AddDays(-1);

        var period1Receipts = await _context.Receipts
            .Where(r => r.UserId == userId && r.ReceiptDate >= period1Start && r.ReceiptDate <= period1End)
            .ToListAsync(cancellationToken);

        var period2Receipts = await _context.Receipts
            .Where(r => r.UserId == userId && r.ReceiptDate >= period2Start && r.ReceiptDate <= period2End)
            .ToListAsync(cancellationToken);

        return new List<ComparisonPeriod>
        {
            new ComparisonPeriod
            {
                PeriodLabel = period1Start.ToString("MMMM yyyy"),
                Amount = period1Receipts.Sum(r => r.TotalAmount),
                ReceiptCount = period1Receipts.Count
            },
            new ComparisonPeriod
            {
                PeriodLabel = period2Start.ToString("MMMM yyyy"),
                Amount = period2Receipts.Sum(r => r.TotalAmount),
                ReceiptCount = period2Receipts.Count
            }
        };
    }
}

