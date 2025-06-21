namespace PaymentGateway.Domain.Entities;

public sealed class Card
{
    public Guid Id { get; private set; }
    public string CardHolderName { get; private set; } = null!;
    public string CardNumber { get; private set; } = null!;
    public string ExpiryMonth { get; private set; } = null!;
    public string ExpiryYear { get; private set; } = null!;
    public string Cvv { get; private set; } = null!;
    public decimal Balance { get; private set; }

    private Card()
    {
        // Required for EF Core
    }

    public Card(
        string cardHolderName,
        string cardNumber,
        string expiryMonth,
        string expiryYear,
        string cvv,
        decimal initialBalance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardHolderName);
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(expiryMonth);
        ArgumentException.ThrowIfNullOrWhiteSpace(expiryYear);
        ArgumentException.ThrowIfNullOrWhiteSpace(cvv);
        ArgumentOutOfRangeException.ThrowIfNegative(initialBalance);

        Id = Guid.NewGuid();
        Balance = initialBalance;
        CardHolderName = cardHolderName;
        CardNumber = cardNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Cvv = cvv;
    }

    public void Debit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        if (Balance < amount)
        {
            throw new InvalidOperationException("Insufficient funds.");
        }

        Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        Balance += amount;
    }
}
