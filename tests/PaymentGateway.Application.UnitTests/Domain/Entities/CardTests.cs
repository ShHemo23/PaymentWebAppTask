using PaymentGateway.Domain.Entities;
using FluentAssertions;

namespace PaymentGateway.Application.UnitTests.Domain.Entities;

public class CardTests
{
    [Fact]
    public void Debit_WithSufficientFunds_ShouldDecreaseBalance()
    {
        // Arrange
        var initialBalance = 100m;
        var debitAmount = 20m;
        var card = new Card(
            cardHolderName: "Test Holder",
            cardNumber: "1234567812345678",
            expiryMonth: "12",
            expiryYear: "2030",
            cvv: "123",
            initialBalance: initialBalance);

        // Act
        card.Debit(debitAmount);

        // Assert
        card.Balance.Should().Be(initialBalance - debitAmount);
    }

    [Fact]
    public void Debit_WithInsufficientFunds_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var initialBalance = 10m;
        var debitAmount = 20m;
        var card = new Card(
            cardHolderName: "Test Holder",
            cardNumber: "1234567812345678",
            expiryMonth: "12",
            expiryYear: "2030",
            cvv: "123",
            initialBalance: initialBalance);

        // Act
        Action act = () => card.Debit(debitAmount);

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds.");
    }

    [Fact]
    public void Debit_WithNegativeAmount_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var initialBalance = 100m;
        var debitAmount = -20m;
        var card = new Card(
            cardHolderName: "Test Holder",
            cardNumber: "1234567812345678",
            expiryMonth: "12",
            expiryYear: "2030",
            cvv: "123",
            initialBalance: initialBalance);

        // Act
        Action act = () => card.Debit(debitAmount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
} 