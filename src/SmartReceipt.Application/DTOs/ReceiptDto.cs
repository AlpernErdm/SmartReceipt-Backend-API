namespace SmartReceipt.Application.DTOs;

public class ReceiptDto
{
    public Guid Id { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = new();
}

public class CreateReceiptDto
{
    public string StoreName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public List<CreateReceiptItemDto> Items { get; set; } = new();
}

public class ReceiptScanResultDto
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? RawOcrText { get; set; }
    public List<ScannedReceiptItemDto> Items { get; set; } = new();
}

public class ScannedReceiptItemDto
{
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Category { get; set; } = "Diğer";
}
