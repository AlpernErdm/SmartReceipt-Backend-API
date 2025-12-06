using SmartReceipt.Domain.Common;

namespace SmartReceipt.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<UsageTracking> UsageTrackings { get; set; } = new List<UsageTracking>();
}
