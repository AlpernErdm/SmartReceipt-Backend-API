using Microsoft.EntityFrameworkCore;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Receipt> Receipts { get; }
    
    DbSet<ReceiptItem> ReceiptItems { get; }
    
    DbSet<User> Users { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}