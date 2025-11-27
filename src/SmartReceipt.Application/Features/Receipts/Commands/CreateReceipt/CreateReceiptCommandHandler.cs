using Mapster;
using MediatR;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Application.Features.Receipts.Commands.CreateReceipt;

public class CreateReceiptCommandHandler : IRequestHandler<CreateReceiptCommand, ReceiptDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAiReceiptScannerService _aiScannerService;

    public CreateReceiptCommandHandler(
        IApplicationDbContext context,
        IAiReceiptScannerService aiScannerService)
    {
        _context = context;
        _aiScannerService = aiScannerService;
    }

    public async Task<ReceiptDto> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        Receipt receipt;

        if (request.UseAiProcessing && request.ImageFile != null)
        {
            var scanResult = await _aiScannerService.ScanReceiptAsync(request.ImageFile, cancellationToken);
            
            if (!scanResult.IsSuccess)
            {
                throw new Exception($"Fiş taraması başarısız: {scanResult.ErrorMessage}");
            }

            receipt = new Receipt
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                StoreName = scanResult.StoreName,
                ReceiptDate = scanResult.ReceiptDate,
                TotalAmount = scanResult.TotalAmount,
                TaxAmount = scanResult.TaxAmount,
                RawOcrText = scanResult.RawOcrText,
                IsProcessed = true,
                CreatedAt = DateTime.UtcNow,
                Items = scanResult.Items.Select(item => new ReceiptItem
                {
                    Id = Guid.NewGuid(),
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Category = item.Category,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };
        }
        else if (request.ManualData != null)
        {
            receipt = new Receipt
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                StoreName = request.ManualData.StoreName,
                ReceiptDate = request.ManualData.ReceiptDate,
                TotalAmount = request.ManualData.TotalAmount,
                TaxAmount = request.ManualData.TaxAmount,
                IsProcessed = true,
                CreatedAt = DateTime.UtcNow,
                Items = request.ManualData.Items.Select(item => new ReceiptItem
                {
                    Id = Guid.NewGuid(),
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    Category = item.Category,
                    CreatedAt = DateTime.UtcNow
                }).ToList()
            };
        }
        else
        {
            throw new ArgumentException("Görsel dosyası veya manuel veri gereklidir.");
        }

        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync(cancellationToken);

        return receipt.Adapt<ReceiptDto>();
    }
}
