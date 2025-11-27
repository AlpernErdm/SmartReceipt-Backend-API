using MediatR;
using SmartReceipt.Application.DTOs;

namespace SmartReceipt.Application.Features.Receipts.Queries.GetReceiptById;

public record GetReceiptByIdQuery(Guid Id, Guid UserId) : IRequest<ReceiptDto?>;

