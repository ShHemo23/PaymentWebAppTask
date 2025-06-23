using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; private set; }
    public Guid CardId { get; set; }
    public Card Card { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public TransactionStatus Status { get; private set; }
    public DateTimeOffset CreatedDate { get; private set; }
    public DateTimeOffset? LastModifiedDate { get; private set; }
    public string PublicTransactionId { get; private set; } = string.Empty;
    public string? RefundCode { get; private set; }
    public DateTimeOffset? RefundCodeExpiryUtc { get; private set; }

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
        Status = TransactionStatus.Authorized;
        CreatedDate = DateTimeOffset.UtcNow;
        PublicTransactionId = string.Empty;
        RefundCode = null;
        RefundCodeExpiryUtc = null;
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

    public void MarkAsRefunded()
    {
        Status = TransactionStatus.Refunded;
        LastModifiedDate = DateTimeOffset.UtcNow;
    }

    public void InitializeRefund(string refundCode, DateTimeOffset expiryUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refundCode);
        RefundCode = refundCode;
        RefundCodeExpiryUtc = expiryUtc;
    }

    public void SetPublicTransactionId(string transactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
        PublicTransactionId = transactionId;
    }

    public void ExpireRefund()
    {
        RefundCode = null;
        RefundCodeExpiryUtc = null;
    }
}
