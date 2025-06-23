using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MediatR;
using PaymentGateway.Application.Features.Cards.Commands;
using PaymentGateway.Application.Features.Payments.Commands;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Infrastructure.Data;
using Xunit;

namespace PaymentGateway.Application.UnitTests.Application;

public class ProcessPaymentCommandHandlerTests
{
    private static (ProcessPaymentCommandHandler handler, PaymentDbContext context) CreateSut()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new PaymentDbContext(options);

        var senderMock = new Mock<ISender>();
        senderMock.Setup(m => m.Send(It.IsAny<ValidateCardCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidateCardResponse(true));

        var auditMock = new Mock<IAuditService>();
        var handler = new ProcessPaymentCommandHandler(senderMock.Object, dbContext, auditMock.Object, NullLogger<ProcessPaymentCommandHandler>.Instance);
        return (handler, dbContext);
    }

    [Fact]
    public async Task Handle_NewCard_SufficientBalance_CreatesAuthorizedTransaction()
    {
        // Arrange
        var (sut, ctx) = CreateSut();
        var cmd = new ProcessPaymentCommand
        {
            CardNumber = "4539682995824395",
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "123",
            Amount = 100m
        };

        // Act
        var resp = await sut.Handle(cmd, default);

        // Assert
        resp.TransactionId.Should().NotBeNullOrEmpty();
        resp.RefundCode.Should().NotBeNullOrEmpty();

        var txn = await ctx.Transactions.Include(t=>t.Card).FirstAsync();
        txn.Status.Should().Be(PaymentGateway.Domain.Enums.TransactionStatus.Authorized);
        txn.Amount.Should().Be(100m);
        txn.Card.Balance.Should().Be(9900m);
    }

    [Fact]
    public async Task Handle_InsufficientFunds_Throws()
    {
        var (sut, ctx) = CreateSut();
        // create card with low balance
        var lowCmd = new ProcessPaymentCommand
        {
            CardNumber = "4539682995824395",
            ExpiryMonth = DateTime.UtcNow.AddMonths(1).Month,
            ExpiryYear = DateTime.UtcNow.AddMonths(1).Year,
            Cvv = "123",
            Amount = 10000m
        };
        // first call to set balance to 0
        await sut.Handle(lowCmd, default);

        // second call should throw insufficient funds
        Func<Task> act = async () => await sut.Handle(lowCmd, default);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_CardNumberWithDifferentFormats_FindsSameCard()
    {
        // Arrange
        var (sut, ctx) = CreateSut();
        var cmd1 = new ProcessPaymentCommand
        {
            CardNumber = "4539-6829-9582-4395", // Valid Luhn
            ExpiryMonth = 12, ExpiryYear = 2030, Cvv = "123", Amount = 100m
        };
        var cmd2 = new ProcessPaymentCommand
        {
            CardNumber = "4539 6829 9582 4395", // Same number, different format
            ExpiryMonth = 12, ExpiryYear = 2030, Cvv = "123", Amount = 50m
        };

        // Act
        await sut.Handle(cmd1, default); // First call creates the card
        await sut.Handle(cmd2, default); // Second call should find the same card

        // Assert
        ctx.Cards.Should().HaveCount(1);
        var card = await ctx.Cards.SingleAsync();
        card.Balance.Should().Be(10000m - 100m - 50m);
    }
} 