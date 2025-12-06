using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Domain.Enums;

namespace SmartReceipt.Application.Features.Subscriptions.Queries.GetUsage;

public record GetUsageQuery(Guid UserId, int? Year = null, int? Month = null) : IRequest<UsageDto>;

public class GetUsageQueryHandler : IRequestHandler<GetUsageQuery, UsageDto>
{
    private readonly IApplicationDbContext _context;

    public GetUsageQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UsageDto> Handle(GetUsageQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;

        // Aktif subscription'ı bul
        var subscription = await _context.Subscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == request.UserId)
            .Where(s => s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trial)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Usage tracking'i bul veya oluştur
        var usageTracking = await _context.UsageTrackings
            .FirstOrDefaultAsync(ut => ut.UserId == request.UserId && ut.Year == year && ut.Month == month, cancellationToken);

        int scanLimit = 10; // Default Free plan limiti
        long storageLimitBytes = 100 * 1024 * 1024; // 100 MB default

        if (subscription != null)
        {
            scanLimit = subscription.SubscriptionPlan.MonthlyScanLimit;
            storageLimitBytes = subscription.SubscriptionPlan.StorageLimitMB * 1024 * 1024;
        }

        var scanCount = usageTracking?.ScanCount ?? 0;
        var storageUsedBytes = usageTracking?.StorageUsedBytes ?? 0;
        var apiCallCount = usageTracking?.ApiCallCount ?? 0;

        var usagePercentage = scanLimit > 0 ? (decimal)scanCount / scanLimit * 100 : 0;
        var isLimitExceeded = scanCount >= scanLimit;

        return new UsageDto
        {
            UserId = request.UserId,
            Year = year,
            Month = month,
            ScanCount = scanCount,
            ScanLimit = scanLimit,
            StorageUsedBytes = storageUsedBytes,
            StorageLimitBytes = storageLimitBytes,
            ApiCallCount = apiCallCount,
            UsagePercentage = usagePercentage,
            IsLimitExceeded = isLimitExceeded
        };
    }
}

