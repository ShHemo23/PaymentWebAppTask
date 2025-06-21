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
            .HasMaxLength(19) // e.g., "xxxx-xxxx-xxxx-xxxx"
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

        // Seed a test card
        builder.HasData(new Card
        {
            Id = Guid.Parse("f9a4a7a0-02a8-4e3a-8671-5f2a1d7f6b8a"),
            CardHolderName = "John Smith",
            CardNumber = "4242-4242-4242-4242",
            ExpiryMonth = "12",
            ExpiryYear = "2030",
            Cvv = "123",
            Balance = 1000.00m
        });
    }
} 