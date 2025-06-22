using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid CardId { get; private set; }
    public decimal Amount { get; private set; }
    public required string Currency { get; init; }
    public TransactionStatus Status { get; private set; }
    public DateTimeOffset CreatedDate { get; private set; }
    public DateTimeOffset? LastModifiedDate { get; private set; }

    private Transaction()
    {
        // Required by EF Core
    }
    
    public Transaction(Guid cardId, decimal amount, string currency)
    {
        if (cardId == Guid.Empty)
        {
            throw new ArgumentException("Card ID cannot be empty.", nameof(cardId));
        }
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        Id = Guid.NewGuid();
        CardId = cardId;
        Amount = amount;
        Currency = currency;
        Status = TransactionStatus.Pending;
        CreatedDate = DateTimeOffset.UtcNow;
    }

    public void MarkAsSuccessful()
    {
        Status = TransactionStatus.Successful;
        LastModifiedDate = DateTimeOffset.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = TransactionStatus.Failed;
        LastModifiedDate = DateTimeOffset.UtcNow;
    }

    // Navigation property
    public Card? Card { get; set; }
}
