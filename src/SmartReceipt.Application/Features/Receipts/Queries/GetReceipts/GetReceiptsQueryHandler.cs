using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Receipts.Queries.GetReceipts;

public class GetReceiptsQueryHandler : IRequestHandler<GetReceiptsQuery, List<ReceiptDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReceiptsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReceiptDto>> Handle(GetReceiptsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Receipts
            .Include(r => r.Items)
            .AsQueryable();

        if (request.FromDate.HasValue)
        {
            query = query.Where(r => r.ReceiptDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(r => r.ReceiptDate <= request.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.StoreName))
        {
            query = query.Where(r => r.StoreName.Contains(request.StoreName));
        }

        var receipts = await query
            .OrderByDescending(r => r.ReceiptDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return receipts.Adapt<List<ReceiptDto>>();
    }
}
