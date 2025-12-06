using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.Currency.TRY);

        builder.Property(b => b.Period)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(b => b.Year)
            .IsRequired();

        builder.Property(b => b.Category)
            .HasConversion<int>();

        builder.Property(b => b.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(b => b.AlertThreshold)
            .HasPrecision(5, 2);

        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => new { b.UserId, b.Year, b.Month, b.Category });

        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

