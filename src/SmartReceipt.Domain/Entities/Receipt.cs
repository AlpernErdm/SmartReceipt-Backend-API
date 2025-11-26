using SmartReceipt.Domain.Common;

namespace SmartReceipt.Domain.Entities;

public class Receipt : BaseEntity
{
    public string StoreName { get; set; } = string.Empty;
    
    public DateTime ReceiptDate { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public decimal TaxAmount { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public string? RawOcrText { get; set; }
    
    public bool IsProcessed { get; set; }
    
    public Guid? UserId { get; set; }
    
    public ICollection<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();
}
