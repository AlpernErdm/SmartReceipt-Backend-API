using SmartReceipt.Domain.Common;

namespace SmartReceipt.Domain.Entities;

public class ReceiptItem : BaseEntity
{
    public string ProductName { get; set; } = string.Empty;
    
    public decimal Quantity { get; set; }
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public string Category { get; set; } = "Diğer";
    
    public Guid ReceiptId { get; set; }
    
    public Receipt Receipt { get; set; } = null!;
}
