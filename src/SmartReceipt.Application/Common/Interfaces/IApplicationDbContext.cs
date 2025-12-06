using Microsoft.EntityFrameworkCore;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Receipt> Receipts { get; }
    
    DbSet<ReceiptItem> ReceiptItems { get; }
    
    DbSet<User> Users { get; }
    
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    
    DbSet<Subscription> Subscriptions { get; }
    
    DbSet<UsageTracking> UsageTrackings { get; }
    
    DbSet<Payment> Payments { get; }
    
    DbSet<Invoice> Invoices { get; }
    
    DbSet<Refund> Refunds { get; }
    
    DbSet<Budget> Budgets { get; }
    
    DbSet<ReceiptAnalysis> ReceiptAnalyses { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}