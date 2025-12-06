using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Infrastructure.Services;

public class UsageLimitService : IUsageLimitService
{
    private readonly IApplicationDbContext _context;

    public UsageLimitService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanUserScanReceiptAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var remainingScans = await GetRemainingScansAsync(userId, cancellationToken);
        return remainingScans > 0;
    }

    public async Task<int> GetRemainingScansAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == userId)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        int scanLimit = 10; // Default Free plan limiti

        if (subscription != null)
        {
            scanLimit = subscription.SubscriptionPlan.MonthlyScanLimit;
        }

        var usageTracking = await _context.UsageTrackings
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.Year == year && ut.Month == month, cancellationToken);

        var scanCount = usageTracking?.ScanCount ?? 0;
        var remaining = Math.Max(0, scanLimit - scanCount);

        return remaining;
    }

    public async Task IncrementScanCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var month = now.Month;

        var usageTracking = await _context.UsageTrackings
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.Year == year && ut.Month == month, cancellationToken);

        if (usageTracking == null)
        {
            var subscription = await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            usageTracking = new Domain.Entities.UsageTracking
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SubscriptionId = subscription?.Id,
                Year = year,
                Month = month,
                ScanCount = 0,
                StorageUsedBytes = 0,
                ApiCallCount = 0,
                CreatedAt = now
            };

            _context.UsageTrackings.Add(usageTracking);
        }

        usageTracking.ScanCount++;
        usageTracking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

