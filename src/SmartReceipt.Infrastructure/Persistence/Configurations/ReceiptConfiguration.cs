using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("Receipts");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.StoreName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ReceiptDate)
            .IsRequired();

        builder.Property(r => r.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(r => r.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(500);

        builder.Property(r => r.RawOcrText)
            .HasColumnType("text");

        builder.Property(r => r.IsProcessed)
            .HasDefaultValue(false);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.HasIndex(r => r.ReceiptDate);
        builder.HasIndex(r => r.StoreName);
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasMany(r => r.Items)
            .WithOne(i => i.Receipt)
            .HasForeignKey(i => i.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
