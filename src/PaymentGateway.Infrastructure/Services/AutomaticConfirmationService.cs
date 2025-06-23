using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Domain.Enums;

namespace PaymentGateway.Infrastructure.Services;

public sealed class AutomaticConfirmationService : BackgroundService
{
    private readonly ILogger<AutomaticConfirmationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AutomaticConfirmationService(ILogger<AutomaticConfirmationService> logger, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automatic Confirmation Service is starting.");

        stoppingToken.Register(() => _logger.LogInformation("Automatic Confirmation Service is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTransactionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing automatic confirmations.");
            }

            // Run every 5 minutes
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("Automatic Confirmation Service has stopped.");
    }

    private async Task ProcessTransactionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var cutoff = DateTimeOffset.UtcNow.AddMinutes(-5);

        // 1. Settle authorized transactions older than cutoff
        var pendingTxns = await context.Transactions
            .Where(t => t.Status == TransactionStatus.Authorized && t.CreatedDate <= cutoff)
            .ToListAsync(cancellationToken);

        foreach (var txn in pendingTxns)
        {
            txn.MarkAsSuccessful();
            _logger.LogInformation("Automatically settled Txn {TxnId}", txn.PublicTransactionId);
        }

        // 2. Expire outdated refund codes (do not alter status)
        var expiredRefunds = await context.Transactions
            .Where(t => t.RefundCodeExpiryUtc.HasValue && t.RefundCodeExpiryUtc < DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var txn in expiredRefunds)
        {
            txn.ExpireRefund();
            _logger.LogInformation("Expired refund code for Txn {TxnId}", txn.PublicTransactionId);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
