using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Services;

public class MlService : IMlService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<MlService> _logger;

    public MlService(
        IApplicationDbContext context,
        ILogger<MlService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string?> SuggestCategoryAsync(string productName, string? storeName, decimal? amount, CancellationToken cancellationToken = default)
    {
        var productLower = productName.ToLower();
        
        if (productLower.Contains("ekmek") || productLower.Contains("süt") || productLower.Contains("yumurta"))
            return ExpenseCategory.Groceries.ToString();
        
        if (productLower.Contains("restoran") || productLower.Contains("cafe"))
            return ExpenseCategory.Restaurant.ToString();
        
        if (productLower.Contains("benzin") || productLower.Contains("yakıt"))
            return ExpenseCategory.Fuel.ToString();
        
        if (productLower.Contains("ilaç") || productLower.Contains("eczane"))
            return ExpenseCategory.Pharmacy.ToString();
        
        if (storeName != null)
        {
            var storeLower = storeName.ToLower();
            if (storeLower.Contains("migros") || storeLower.Contains("a101") || storeLower.Contains("bim"))
                return ExpenseCategory.Groceries.ToString();
        }
        
        return null;
    }

    public async Task<bool> DetectRecurringExpenseAsync(Guid userId, Guid receiptId, CancellationToken cancellationToken = default)
    {
        var receipt = await _context.Receipts
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == receiptId && r.UserId == userId, cancellationToken);

        if (receipt == null)
            return false;

        var similarReceipts = await _context.Receipts
            .Where(r => r.UserId == userId 
                && r.StoreName == receipt.StoreName
                && r.Id != receiptId
                && Math.Abs((double)(r.TotalAmount - receipt.TotalAmount)) < (double)receipt.TotalAmount * 0.2)
            .OrderByDescending(r => r.ReceiptDate)
            .Take(5)
            .ToListAsync(cancellationToken);

        if (similarReceipts.Count >= 3)
        {
            var dates = similarReceipts.Select(r => r.ReceiptDate).OrderBy(d => d).ToList();
            var intervals = new List<int>();
            
            for (int i = 1; i < dates.Count; i++)
            {
                intervals.Add((dates[i] - dates[i - 1]).Days);
            }
            
            if (intervals.Count >= 2 && intervals.All(i => Math.Abs(i - intervals[0]) <= 7))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<FraudDetectionResult> DetectFraudAsync(Guid receiptId, CancellationToken cancellationToken = default)
    {
        var receipt = await _context.Receipts
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == receiptId, cancellationToken);

        if (receipt == null)
        {
            return new FraudDetectionResult
            {
                IsFraudulent = false,
                ConfidenceScore = 0
            };
        }

        var flags = new List<string>();
        decimal confidenceScore = 0;

        var calculatedTotal = receipt.Items.Sum(i => i.TotalPrice);
        if (Math.Abs(calculatedTotal - receipt.TotalAmount) > 0.01m)
        {
            flags.Add("Total amount mismatch");
            confidenceScore += 30;
        }

        if (receipt.TotalAmount > 100000m)
        {
            flags.Add("Unusually high amount");
            confidenceScore += 20;
        }

        if (receipt.Items.Any(i => i.Quantity < 0 || i.UnitPrice < 0 || i.TotalPrice < 0))
        {
            flags.Add("Negative values detected");
            confidenceScore += 50;
        }

        if (receipt.ReceiptDate > DateTime.UtcNow.AddDays(1))
        {
            flags.Add("Future date");
            confidenceScore += 40;
        }

        return new FraudDetectionResult
        {
            IsFraudulent = confidenceScore >= 50,
            ConfidenceScore = Math.Min(confidenceScore, 100),
            Reason = flags.Any() ? string.Join(", ", flags) : null,
            Flags = flags
        };
    }
}

