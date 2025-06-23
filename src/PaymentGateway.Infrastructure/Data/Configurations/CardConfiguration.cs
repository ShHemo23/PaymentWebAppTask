using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Entities;

namespace PaymentGateway.Infrastructure.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CardHolderName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CardNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(c => c.ExpiryMonth)
            .HasMaxLength(2)
            .IsRequired();
        
        builder.Property(c => c.ExpiryYear)
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(c => c.Cvv)
            .HasMaxLength(4)
            .IsRequired();

        builder.Property(c => c.Balance)
            .HasColumnType("decimal(18,2)");
        
        builder.Property(c => c.AvailableBalance)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Seed a test card using constructor
        var testCard = new Card(
            "Test Card Holder",
            cardNumber: "4242-4242-4242-4242",
            expiryMonth: "12",
            expiryYear: "2025",
            cvv: "123",
            initialBalance: 1000.00m);

        builder.HasData(testCard);
    }
} 