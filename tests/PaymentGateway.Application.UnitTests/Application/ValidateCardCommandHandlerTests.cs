using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentGateway.Application.Features.Cards.Commands;
using Xunit;

namespace PaymentGateway.Application.UnitTests.Application;

public class ValidateCardCommandHandlerTests
{
    private static ValidateCardCommandHandler CreateHandler()
    {
        return new ValidateCardCommandHandler(NullLogger<ValidateCardCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ValidCard_ReturnsValid()
    {
        // Arrange
        var cmd = new ValidateCardCommand
        {
            CardNumber = "4539682995824395", // valid Visa number
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "123"
        };
        var sut = CreateHandler();

        // Act
        var result = await sut.Handle(cmd, default);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Message.Should().BeNull();
    }

    [Fact]
    public async Task Handle_InvalidLuhn_ReturnsInvalid()
    {
        var cmd = new ValidateCardCommand
        {
            CardNumber = "1111111111111111",
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "123"
        };
        var sut = CreateHandler();
        var result = await sut.Handle(cmd, default);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Expired_ReturnsInvalid()
    {
        var cmd = new ValidateCardCommand
        {
            CardNumber = "4539682995824395",
            ExpiryMonth = DateTime.UtcNow.AddMonths(-1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(-1).Year,
            Cvv = "123"
        };
        var sut = CreateHandler();
        var result = await sut.Handle(cmd, default);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidCvv_ReturnsInvalid()
    {
        var cmd = new ValidateCardCommand
        {
            CardNumber = "4539682995824395",
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "12" // too short
        };
        var sut = CreateHandler();
        var result = await sut.Handle(cmd, default);
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("4539 6829 9582 4395")]
    [InlineData("4539-6829-9582-4395")]
    public async Task Handle_CardNumberWithSeparators_ReturnsValid(string cardNumber)
    {
        var cmd = new ValidateCardCommand
        {
            CardNumber = cardNumber,
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "999"
        };
        var sut = CreateHandler();
        var result = await sut.Handle(cmd, default);
        result.IsValid.Should().BeTrue();
    }
} 