using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Id)
            .ValueGeneratedOnAdd();

        builder.Property(sp => sp.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sp => sp.Description)
            .HasMaxLength(500);

        builder.Property(sp => sp.PlanType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(sp => sp.MonthlyPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sp => sp.YearlyPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(sp => sp.MonthlyScanLimit)
            .IsRequired();

        builder.Property(sp => sp.StorageLimitMB)
            .IsRequired();

        builder.Property(sp => sp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(sp => sp.PlanType)
            .IsUnique();

        builder.HasMany(sp => sp.Subscriptions)
            .WithOne(s => s.SubscriptionPlan)
            .HasForeignKey(s => s.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

