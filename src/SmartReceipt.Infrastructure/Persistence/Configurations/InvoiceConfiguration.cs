using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartReceipt.Domain.Entities;

namespace SmartReceipt.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .ValueGeneratedOnAdd();

        builder.Property(i => i.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.TaxAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(i => i.Currency)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.Currency.TRY);

        builder.Property(i => i.IssueDate)
            .IsRequired();

        builder.Property(i => i.DueDate)
            .IsRequired();

        builder.Property(i => i.IsPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.BillingAddress)
            .HasMaxLength(500);

        builder.Property(i => i.TaxNumber)
            .HasMaxLength(50);

        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.InvoiceNumber)
            .IsUnique();
        builder.HasIndex(i => i.IssueDate);
        builder.HasIndex(i => i.IsPaid);

        builder.HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Subscription)
            .WithMany()
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

