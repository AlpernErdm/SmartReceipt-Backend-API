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
    private readonly IUsageLimitService _usageLimitService;

    public CreateReceiptCommandHandler(
        IApplicationDbContext context,
        IAiReceiptScannerService aiScannerService,
        IUsageLimitService usageLimitService)
    {
        _context = context;
        _aiScannerService = aiScannerService;
        _usageLimitService = usageLimitService;
    }

    public async Task<ReceiptDto> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        if (request.UseAiProcessing && request.ImageFile != null)
        {
            var canScan = await _usageLimitService.CanUserScanReceiptAsync(request.UserId, cancellationToken);
            if (!canScan)
            {
                var remaining = await _usageLimitService.GetRemainingScansAsync(request.UserId, cancellationToken);
                throw new Exception($"Aylık tarama limitinize ulaştınız. Kalan tarama hakkı: {remaining}. Lütfen planınızı yükseltin.");
            }
        }

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

        if (request.UseAiProcessing && request.ImageFile != null)
        {
            await _usageLimitService.IncrementScanCountAsync(request.UserId, cancellationToken);
        }

        return receipt.Adapt<ReceiptDto>();
    }
}
