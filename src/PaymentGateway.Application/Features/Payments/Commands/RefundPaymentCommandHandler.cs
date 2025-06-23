using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Features.Payments.Commands;

internal sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;
    private readonly IAuditService _auditService;

    public RefundPaymentCommandHandler(IApplicationDbContext context, ILogger<RefundPaymentCommandHandler> logger, IAuditService auditService)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(auditService);
        _context = context;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<Unit> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var txn = await _context.Transactions.Include(t => t.Card)
            .FirstOrDefaultAsync(t => t.PublicTransactionId == request.TransactionId, cancellationToken);

        if (txn == null)
        {
            await _auditService.LogAsync(new AuditLog(null, "Refund", "Failure", request.TransactionId, null, null));
            throw new KeyNotFoundException("Transaction not found");
        }

        if (txn.Status != TransactionStatus.Authorized)
        {
            await _auditService.LogAsync(new AuditLog(null, "Refund", "Failure", txn.Id.ToString(), null, null));
            throw new InvalidOperationException("Refund not allowed for this transaction status");
        }

        if (txn.RefundCode != request.RefundCode)
        {
            await _auditService.LogAsync(new AuditLog(null, "Refund", "Failure", txn.Id.ToString(), null, null));
            throw new InvalidOperationException("Invalid refund code");
        }

        if (txn.RefundCodeExpiryUtc.HasValue && DateTime.UtcNow > txn.RefundCodeExpiryUtc.Value)
        {
            await _auditService.LogAsync(new AuditLog(null, "Refund", "Failure", txn.Id.ToString(), null, null));
            throw new InvalidOperationException("Refund code expired");
        }

        // perform refund
        txn.MarkAsRefunded();
        txn.Card?.Credit(txn.Amount);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Refund failed: concurrency conflict");
            await _auditService.LogAsync(new AuditLog(null, "Refund", "Failure", txn.Id.ToString(), null, null));
            throw new InvalidOperationException("An error occurred while processing the refund. Please try again later.");
        }

        _logger.LogInformation("Refund successful for {TxnId}", request.TransactionId);
        await _auditService.LogAsync(new AuditLog(null, "Refund", "Success", txn.Id.ToString(), null, null));
        return Unit.Value;
    }
} 