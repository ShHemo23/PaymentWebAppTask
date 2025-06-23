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

            // Run every hour
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("Automatic Confirmation Service has stopped.");
    }

    private async Task ProcessTransactionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        var today = DateTime.UtcNow.Date;

        // 1. Settle authorized transactions from previous days
        var pendingTxns = await context.Transactions
            .Where(t => t.Status == TransactionStatus.Authorized && t.CreatedDate.Date < today)
            .ToListAsync(cancellationToken);

        var confirmedCount = 0;
        foreach (var txn in pendingTxns)
        {
            txn.MarkAsSuccessful();
            confirmedCount++;
        }

        if (confirmedCount > 0)
        {
            _logger.LogInformation("Automatically confirmed {Count} transactions from previous days", confirmedCount);
        }

        // 2. Expire outdated refund codes (do not alter status)
        var expiredRefunds = await context.Transactions
            .Where(t => t.RefundCodeExpiryUtc.HasValue && t.RefundCodeExpiryUtc < DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        var expiredCount = 0;
        foreach (var txn in expiredRefunds)
        {
            txn.ExpireRefund();
            expiredCount++;
        }

        if (expiredCount > 0)
        {
            _logger.LogInformation("Expired {Count} refund codes", expiredCount);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}