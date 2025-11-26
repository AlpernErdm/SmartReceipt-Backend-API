using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Receipts.Queries.GetReceiptById;

public class GetReceiptByIdQueryHandler : IRequestHandler<GetReceiptByIdQuery, ReceiptDto?>
{
    private readonly IApplicationDbContext _context;

    public GetReceiptByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReceiptDto?> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _context.Receipts
            .Include(r => r.Items)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        return receipt?.Adapt<ReceiptDto>();
    }
}

