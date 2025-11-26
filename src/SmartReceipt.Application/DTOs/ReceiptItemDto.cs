namespace SmartReceipt.Application.DTOs;

public class ReceiptItemDto
{
    public Guid Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Category { get; set; } = "Diğer";
    public Guid ReceiptId { get; set; }
}

public class CreateReceiptItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Category { get; set; } = "Diğer";
}

