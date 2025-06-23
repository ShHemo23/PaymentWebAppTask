using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(t => t.PublicTransactionId)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.RefundCode)
            .HasMaxLength(4);

        builder.Property(t => t.RefundCodeExpiryUtc);

        builder.Property(t => t.Status)
            .HasConversion(
                v => v.ToString(),
                v => (TransactionStatus)Enum.Parse(typeof(TransactionStatus), v));

        builder.HasOne(t => t.Card)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CardId);
    }
} 