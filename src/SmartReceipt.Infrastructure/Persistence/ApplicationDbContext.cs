using Microsoft.EntityFrameworkCore;
using SmartReceipt.Application.Common.Interfaces;
using SmartReceipt.Domain.Common;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Receipt> Receipts => Set<Receipt>();
    
    public DbSet<ReceiptItem> ReceiptItems => Set<ReceiptItem>();
    
    public DbSet<User> Users => Set<User>();
    
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    
    public DbSet<UsageTracking> UsageTrackings => Set<UsageTracking>();
    
    public DbSet<Payment> Payments => Set<Payment>();
    
    public DbSet<Invoice> Invoices => Set<Invoice>();
    
    public DbSet<Refund> Refunds => Set<Refund>();
    
    public DbSet<Budget> Budgets => Set<Budget>();
    
    public DbSet<ReceiptAnalysis> ReceiptAnalyses => Set<ReceiptAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}