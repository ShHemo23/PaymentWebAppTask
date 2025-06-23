using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PaymentGateway.Application.Features.Payments.Commands;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Enums;
using PaymentGateway.Infrastructure.Data;
using Xunit;

namespace PaymentGateway.Application.UnitTests.Application;

public class RefundPaymentCommandHandlerTests
{
    private static (RefundPaymentCommandHandler handler, PaymentDbContext ctx, Guid cardId, string txnId, string refundCode) Seed()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new PaymentDbContext(options);

        // seed card and transaction
        var card = new PaymentGateway.Domain.Entities.Card("name", "hash", "01", "2030", "***", 1000m);
        db.Cards.Add(card);
        var txn = new PaymentGateway.Domain.Entities.Transaction(card.Id, 100m, "AED");
        txn.SetPublicTransactionId("TXN123456");
        txn.InitializeRefund("9999", DateTime.UtcNow.AddHours(1));
        db.Transactions.Add(txn);
        db.SaveChanges();

        var auditMock = new Mock<IAuditService>();
        auditMock.Setup(a => a.LogAsync(It.IsAny<PaymentGateway.Domain.Entities.AuditLog>()))
                  .Returns(Task.CompletedTask);

        var handler = new RefundPaymentCommandHandler(db, NullLogger<RefundPaymentCommandHandler>.Instance, auditMock.Object);
        return (handler, db, card.Id, txn.PublicTransactionId, "9999");
    }

    [Fact]
    public async Task Handle_ValidRequest_RefundsSuccessfully()
    {
        var (sut, db, cardId, txnId, refundCode) = Seed();
        var cmd = new RefundPaymentCommand(txnId, refundCode);
        await sut.Handle(cmd, default);

        var txn = await db.Transactions.FirstAsync();
        txn.Status.Should().Be(TransactionStatus.Refunded);
        (await db.Cards.FirstAsync()).Balance.Should().Be(1100m);
    }

    [Fact]
    public async Task Handle_InvalidRefundCode_Throws()
    {
        var (sut, db, cardId, txnId, refundCode) = Seed();
        var cmd = new RefundPaymentCommand(txnId, "0000");
        Func<Task> act = async () => await sut.Handle(cmd, default);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
} 