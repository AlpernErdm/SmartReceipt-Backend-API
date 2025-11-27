using MediatR;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Receipts.Queries.GetReceipts;

public record GetReceiptsQuery : IRequest<List<ReceiptDto>>
{
    public Guid UserId { get; init; }
    
    public DateTime? FromDate { get; init; }
    
    public DateTime? ToDate { get; init; }
    
    public string? StoreName { get; init; }
    
    public int PageNumber { get; init; } = 1;
    
    public int PageSize { get; init; } = 10;
}