using Microsoft.AspNetCore.Http;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IAiReceiptScannerService
{
    Task<ReceiptScanResultDto> ScanReceiptAsync(IFormFile imageFile, CancellationToken cancellationToken = default);
    
    Task<ReceiptScanResultDto> ScanReceiptFromBase64Async(string base64Image, CancellationToken cancellationToken = default);
}
