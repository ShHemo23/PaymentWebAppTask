namespace PaymentGateway.Domain.Entities;

public sealed class Card
{
    private Card()
    {
        // Required by EF Core
        Id = Guid.NewGuid();
        CardHolderName = string.Empty;
        CardNumber = string.Empty;
        ExpiryMonth = string.Empty;
        ExpiryYear = string.Empty;
        Cvv = string.Empty;
        RowVersion = [];
    }

    public Card(string cardHolderName, string cardNumber, string expiryMonth, string expiryYear, string cvv, decimal initialBalance = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardHolderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(expiryMonth);
        ArgumentException.ThrowIfNullOrWhiteSpace(expiryYear);
        ArgumentException.ThrowIfNullOrWhiteSpace(cvv);
        ArgumentOutOfRangeException.ThrowIfNegative(initialBalance);

        Id = Guid.NewGuid();
        CardHolderName = cardHolderName;
        CardNumber = cardNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Cvv = cvv;
        Balance = initialBalance;
        AvailableBalance = initialBalance;
        RowVersion = [];
    }

    public Guid Id { get; private set; }
    public string CardHolderName { get; private set; }
    public string CardNumber { get; private set; }
    public string ExpiryMonth { get; private set; }
    public string ExpiryYear { get; private set; }
    public string Cvv { get; private set; }
    public decimal Balance { get; private set; }
    public decimal AvailableBalance { get; set; }
    public byte[] RowVersion { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public void Debit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds.");
        }

        Balance -= amount;
        AvailableBalance -= amount;
    }

    public void Credit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Balance += amount;
        AvailableBalance += amount;
    }
}
