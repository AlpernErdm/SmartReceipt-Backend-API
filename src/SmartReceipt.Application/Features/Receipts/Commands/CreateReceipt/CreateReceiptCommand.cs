using MediatR;
using Microsoft.AspNetCore.Http;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Receipts.Commands.CreateReceipt;

public record CreateReceiptCommand : IRequest<ReceiptDto>
{
    public IFormFile? ImageFile { get; init; }
    
    public CreateReceiptDto? ManualData { get; init; }
    
    public bool UseAiProcessing { get; init; } = true;
}
