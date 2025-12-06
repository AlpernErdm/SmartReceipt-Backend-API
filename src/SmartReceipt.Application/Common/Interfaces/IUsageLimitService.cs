namespace SmartReceipt.Application.Common.Interfaces;

public interface IUsageLimitService
{
    Task<bool> CanUserScanReceiptAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task IncrementScanCountAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<int> GetRemainingScansAsync(Guid userId, CancellationToken cancellationToken = default);
}

