using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class ReceiptAnalysisConfiguration : IEntityTypeConfiguration<ReceiptAnalysis>
{
    public void Configure(EntityTypeBuilder<ReceiptAnalysis> builder)
    {
        builder.ToTable("ReceiptAnalyses");

        builder.HasKey(ra => ra.Id);

        builder.Property(ra => ra.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ra => ra.TotalByCategory)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ra => ra.PrimaryCategory)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ra => ra.CategoryBreakdownJson)
            .HasColumnType("text");

        builder.Property(ra => ra.IsRecurring)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ra => ra.AverageAmount)
            .HasPrecision(18, 2);

        builder.Property(ra => ra.VerificationScore)
            .HasPrecision(5, 2)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ra => ra.IsVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ra => ra.IsFraudulent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ra => ra.FraudReason)
            .HasMaxLength(500);

        builder.Property(ra => ra.SuggestedCategory)
            .HasMaxLength(100);

        builder.HasIndex(ra => ra.ReceiptId)
            .IsUnique();

        builder.HasOne(ra => ra.Receipt)
            .WithOne()
            .HasForeignKey<ReceiptAnalysis>(ra => ra.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

