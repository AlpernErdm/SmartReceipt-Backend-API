using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class UsageTrackingConfiguration : IEntityTypeConfiguration<UsageTracking>
{
    public void Configure(EntityTypeBuilder<UsageTracking> builder)
    {
        builder.ToTable("UsageTrackings");

        builder.HasKey(ut => ut.Id);

        builder.Property(ut => ut.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ut => ut.Year)
            .IsRequired();

        builder.Property(ut => ut.Month)
            .IsRequired();

        builder.Property(ut => ut.ScanCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ut => ut.StorageUsedBytes)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ut => ut.ApiCallCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(ut => new { ut.UserId, ut.Year, ut.Month })
            .IsUnique();

        builder.HasIndex(ut => ut.UserId);
        builder.HasIndex(ut => new { ut.Year, ut.Month });

        builder.HasOne(ut => ut.User)
            .WithMany(u => u.UsageTrackings)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ut => ut.Subscription)
            .WithMany(s => s.UsageTrackings)
            .HasForeignKey(ut => ut.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

