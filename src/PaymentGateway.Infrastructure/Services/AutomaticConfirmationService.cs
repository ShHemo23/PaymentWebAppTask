using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Infrastructure.Services;

public sealed class AutomaticConfirmationService : BackgroundService
{
    private readonly ILogger<AutomaticConfirmationService> _logger;

    public AutomaticConfirmationService(ILogger<AutomaticConfirmationService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automatic Confirmation Service is starting.");

        stoppingToken.Register(() => _logger.LogInformation("Automatic Confirmation Service is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Automatic Confirmation Service is doing background work.");
            
            // In a real system, this would query for pending transactions and confirm them.
            
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }

        _logger.LogInformation("Automatic Confirmation Service has stopped.");
    }
}
