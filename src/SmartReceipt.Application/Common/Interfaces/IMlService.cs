using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IMlService
{
    Task<string?> SuggestCategoryAsync(string productName, string? storeName, decimal? amount, CancellationToken cancellationToken = default);
    
    Task<bool> DetectRecurringExpenseAsync(Guid userId, Guid receiptId, CancellationToken cancellationToken = default);
    
    Task<FraudDetectionResult> DetectFraudAsync(Guid receiptId, CancellationToken cancellationToken = default);
}

public class FraudDetectionResult
{
    public bool IsFraudulent { get; set; }
    public decimal ConfidenceScore { get; set; } // 0-100
    public string? Reason { get; set; }
    public List<string> Flags { get; set; } = new();
}

