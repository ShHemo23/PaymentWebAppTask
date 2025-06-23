using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Features.Cards.Commands;
using PaymentGateway.Application.Interfaces;
using PaymentGateway.Domain.Entities;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Application.Features.Payments.Commands;

internal sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponse>
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(ISender sender, IApplicationDbContext context, IAuditService auditService, ILogger<ProcessPaymentCommandHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(auditService);
        ArgumentNullException.ThrowIfNull(logger);
        _sender = sender;
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PaymentResponse> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        string cardHash;
        Transaction? transaction = null;
        try
        {
            // 1. Validate card using existing command/handler
            var validationResult = await _sender.Send(new ValidateCardCommand
            {
                CardNumber = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Cvv = request.Cvv
            }, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Payment validation failed: {Reason}", validationResult.Message);
                throw new InvalidOperationException(validationResult.Message ?? "Card validation failed");
            }

            // 2. Get or create the card (hashed number)
            var sanitized = string.Concat(request.CardNumber.Where(char.IsDigit));
            cardHash = ComputeSha256Hash(sanitized);
            var card = await _context.Cards.FirstOrDefaultAsync(c => c.CardNumber == cardHash, cancellationToken);
            if (card == null)
            {
                card = new Card(
                    cardHolderName: "N/A",
                    cardNumber: cardHash,
                    expiryMonth: request.ExpiryMonth.ToString("D2"),
                    expiryYear: request.ExpiryYear.ToString(),
                    cvv: "***",
                    initialBalance: 10_000m);
                await _context.Cards.AddAsync(card, cancellationToken);
            }

            // 3. Check balance
            if (card.Balance < request.Amount)
            {
                _logger.LogWarning("Insufficient funds for card {CardHash}", cardHash);
                throw new InvalidOperationException("Insufficient funds");
            }

            transaction = new Transaction(card.Id, request.Amount, "AED");
            
            var publicTransactionId = GenerateRandomString(10, 20);
            var refundCode = RandomNumberGenerator.GetInt32(1000, 10000).ToString("D4");
            var refundExpiry = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

            transaction.SetPublicTransactionId(publicTransactionId);
            transaction.InitializeRefund(refundCode, refundExpiry);
            
            card.Debit(request.Amount);

            await _context.Transactions.AddAsync(transaction, cancellationToken);
            
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                transaction.MarkAsFailed();
                await _context.SaveChangesAsync(cancellationToken);
                // Log the exception details...
                throw new InvalidOperationException("The transaction could not be completed due to a concurrency conflict. Please try again.", ex);
            }
            
            await _auditService.LogAsync(new AuditLog(null, "ProcessPayment", "Success", transaction.Id.ToString(), null, transaction.Amount.ToString()));

            _logger.LogInformation("Payment processed successfully TxnId {TxnId}", transaction.PublicTransactionId);
            return new PaymentResponse(transaction.PublicTransactionId, transaction.RefundCode);
        }
        catch (Exception ex)
        {
            if (ex is not DbUpdateConcurrencyException && transaction != null)
            {
                transaction.MarkAsFailed();
                await _context.SaveChangesAsync(cancellationToken);
            }
            await _auditService.LogAsync(new AuditLog(null, "ProcessPayment", "Failure", transaction?.Id.ToString(), null, ex.Message));
            throw;
        }
    }

    private static string ComputeSha256Hash(string raw)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerateRandomString(int minLength, int maxLength)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        int length = RandomNumberGenerator.GetInt32(minLength, maxLength + 1);
        var buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }
        return new string(buffer);
    }
} 