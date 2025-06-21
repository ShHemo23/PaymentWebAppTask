using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid CardId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Transaction()
    {
        // Required for EF Core
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
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsSuccessful()
    {
        Status = TransactionStatus.Successful;
    }

    public void MarkAsFailed()
    {
        Status = TransactionStatus.Failed;
    }

    // Navigation property
    public Card? Card { get; set; }
}
