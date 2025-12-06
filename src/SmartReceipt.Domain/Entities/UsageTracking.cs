using SmartReceipt.Domain.Common;

namespace SmartReceipt.Domain.Entities;

public class UsageTracking : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    
    public int Year { get; set; }
    
    public int Month { get; set; }
    
    public int ScanCount { get; set; } // Bu ay taranan fiş sayısı
    
    public long StorageUsedBytes { get; set; } // Kullanılan depolama (bytes)
    
    public int ApiCallCount { get; set; } // API çağrı sayısı
    
    // Unique constraint: Bir kullanıcı için aynı yıl/ay kombinasyonu sadece bir kez olabilir
}

