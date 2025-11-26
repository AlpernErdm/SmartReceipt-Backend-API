using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class ReceiptItemConfiguration : IEntityTypeConfiguration<ReceiptItem>
{
    public void Configure(EntityTypeBuilder<ReceiptItem> builder)
    {
        builder.ToTable("ReceiptItems");

        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ri => ri.ProductName)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(ri => ri.Quantity)
            .HasPrecision(10, 3)
            .IsRequired();

        builder.Property(ri => ri.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ri => ri.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(ri => ri.Category)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Diğer");

        builder.Property(ri => ri.CreatedAt)
            .IsRequired();

        builder.Property(ri => ri.UpdatedAt);

        builder.HasIndex(ri => ri.ReceiptId);
        builder.HasIndex(ri => ri.Category);
        builder.HasIndex(ri => ri.ProductName);
    }
}
